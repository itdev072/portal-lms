using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Services;

namespace Portal.Api.Controllers;

[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentController : ApiControllerBase
{
    private readonly GradingService _grading;

    public StudentController(AppData data, GradingService grading) : base(data)
    {
        _grading = grading;
    }

    /// <summary>Enroll into the first available class for a course (self-serve, like a marketplace "Enroll" button).</summary>
    [HttpPost("enroll/{courseId}")]
    public IActionResult Enroll(string courseId)
    {
        var cls = Data.ClassSections.Where(c => c.LevelId == courseId).FirstOrDefault();
        if (cls == null) return NotFound(new { message = "No class is currently scheduled for this course." });

        var already = Data.Enrollments.Where(e => e.StudentId == CurrentUserId && e.ClassSectionId == cls.Id).Any();
        if (already) return Ok(new { message = "Already enrolled.", classId = cls.Id });

        Data.Enrollments.Upsert(new Enrollment { StudentId = CurrentUserId!, ClassSectionId = cls.Id });
        return Ok(new { message = "Enrolled!", classId = cls.Id });
    }

    [HttpGet("my-courses")]
    public IActionResult MyCourses()
    {
        var classIds = Data.Enrollments.Where(e => e.StudentId == CurrentUserId).Select(e => e.ClassSectionId).ToHashSet();
        var courseIds = Data.ClassSections.Where(c => classIds.Contains(c.Id)).Select(c => c.LevelId).Distinct();
        var courses = courseIds.Select(id => Data.Levels.Find(id)).Where(l => l != null);
        return Ok(courses);
    }

    /// <summary>Which of this course's lessons the student has completed — drives the
    /// green-checkmark state in the course player's curriculum sidebar.</summary>
    [HttpGet("courses/{courseId}/progress")]
    public IActionResult CourseProgress(string courseId)
    {
        var completedLessonIds = Data.Progress
            .Where(p => p.StudentId == CurrentUserId && p.LevelId == courseId && p.Status == ProgressStatus.Completed && p.LessonId != null)
            .Select(p => p.LessonId!)
            .ToList();
        return Ok(new { completedLessonIds });
    }

    [HttpPost("quiz/{quizId}/submit")]
    public IActionResult SubmitQuiz(string quizId, [FromBody] int selectedIndex)
    {
        var quiz = Data.QuizItems.Find(quizId);
        if (quiz == null) return NotFound();
        var attempt = _grading.GradeQuiz(CurrentUserId!, quiz, selectedIndex);
        MaybeCompleteLesson(quiz.LessonId);
        return Ok(new { attempt.IsCorrect, attempt.XpAwarded, quiz.ExplanationKm, quiz.CorrectIndex });
    }

    [HttpPost("challenge/{challengeId}/submit")]
    public IActionResult SubmitChallenge(string challengeId, [FromBody] string output)
    {
        var challenge = Data.CodingChallenges.Find(challengeId);
        if (challenge == null) return NotFound();
        var (allPassed, perTestCase) = _grading.GradeChallenge(CurrentUserId!, challenge, output);
        if (allPassed) MaybeCompleteLesson(challenge.LessonId);
        return Ok(new { allPassed, perTestCase });
    }

    private void MaybeCompleteLesson(string lessonId)
    {
        var lesson = Data.Lessons.Find(lessonId);
        if (lesson == null) return;

        var existing = Data.Progress.Where(p => p.StudentId == CurrentUserId && p.LessonId == lessonId).FirstOrDefault();
        if (existing?.Status == ProgressStatus.Completed) return;

        var record = existing ?? new StudentProgress { StudentId = CurrentUserId!, LevelId = lesson.LevelId, LessonId = lessonId };
        record.Status = ProgressStatus.Completed;
        record.XpEarned = Math.Max(record.XpEarned, lesson.XpValue);
        record.CompletedAt = DateTimeOffset.UtcNow;
        Data.Progress.Upsert(record);
    }
}
