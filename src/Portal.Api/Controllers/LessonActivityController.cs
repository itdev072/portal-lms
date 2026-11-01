using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Models.Dto;

namespace Portal.Api.Controllers;

/// <summary>Everything tied to a lesson's video player: timestamped notes (private, per
/// student) and the Q&amp;A tab (shared, any signed-in role can ask/reply).</summary>
[Route("api/lessons/{lessonId}")]
[Authorize]
public class LessonActivityController : ApiControllerBase
{
    public LessonActivityController(AppData data) : base(data) { }

    // ---- Notes (student's own, private) ----

    [HttpGet("notes")]
    public IActionResult MyNotes(string lessonId)
    {
        var notes = Data.LessonNotes.Where(n => n.LessonId == lessonId && n.StudentId == CurrentUserId)
            .OrderBy(n => n.TimestampSeconds).Select(LessonNoteDto.From);
        return Ok(notes);
    }

    [HttpPost("notes")]
    public IActionResult AddNote(string lessonId, SaveNoteRequest req)
    {
        var note = new LessonNote
        {
            LessonId = lessonId,
            StudentId = CurrentUserId!,
            TimestampSeconds = req.TimestampSeconds,
            Text = req.Text
        };
        Data.LessonNotes.Upsert(note);
        return Ok(LessonNoteDto.From(note));
    }

    [HttpDelete("notes/{noteId}")]
    public IActionResult DeleteNote(string lessonId, string noteId)
    {
        var note = Data.LessonNotes.Find(noteId);
        if (note == null || note.StudentId != CurrentUserId) return NotFound();
        Data.LessonNotes.Delete(noteId);
        return Ok();
    }

    // ---- Q&A (shared thread on the lesson) ----

    [HttpGet("questions")]
    public IActionResult Questions(string lessonId)
    {
        var questions = Data.LessonQuestions.Where(q => q.LessonId == lessonId)
            .OrderBy(q => q.CreatedAt).Select(LessonQuestionDto.From);
        return Ok(questions);
    }

    [HttpPost("questions")]
    public IActionResult Ask(string lessonId, AskQuestionRequest req)
    {
        var me = CurrentUser;
        var q = new LessonQuestion
        {
            LessonId = lessonId,
            AuthorId = CurrentUserId!,
            AuthorName = me?.DisplayName ?? "Someone",
            Text = req.Text,
            ParentId = req.ParentId
        };
        Data.LessonQuestions.Upsert(q);
        return Ok(LessonQuestionDto.From(q));
    }

    // ---- Mark the video as watched / lesson complete ----

    [HttpPost("complete")]
    [Authorize(Roles = "Student")]
    public IActionResult Complete(string lessonId)
    {
        var lesson = Data.Lessons.Find(lessonId);
        if (lesson == null) return NotFound();

        var existing = Data.Progress.Where(p => p.StudentId == CurrentUserId && p.LessonId == lessonId).FirstOrDefault();
        if (existing?.Status == ProgressStatus.Completed) return Ok(existing);

        var record = existing ?? new StudentProgress { StudentId = CurrentUserId!, LevelId = lesson.LevelId, LessonId = lessonId };
        record.Status = ProgressStatus.Completed;
        record.XpEarned = Math.Max(record.XpEarned, lesson.XpValue);
        record.CompletedAt = DateTimeOffset.UtcNow;
        Data.Progress.Upsert(record);
        return Ok(record);
    }
}
