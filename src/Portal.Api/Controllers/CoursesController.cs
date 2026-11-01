using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Models.Dto;

namespace Portal.Api.Controllers;

[Route("api/courses")]
public class CoursesController : ApiControllerBase
{
    public CoursesController(AppData data) : base(data) { }

    private CourseDto ToDto(Level l)
    {
        var lessonCount = Data.Lessons.Where(x => x.LevelId == l.Id).Count;
        var enrolledCount = Data.ClassSections.Where(c => c.LevelId == l.Id)
            .SelectMany(c => Data.Enrollments.Where(e => e.ClassSectionId == c.Id))
            .Count();
        return CourseDto.From(l, lessonCount, enrolledCount);
    }

    /// <summary>Public catalog — the landing page. Published courses only.</summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult List([FromQuery] string? q, [FromQuery] CourseCategory? category)
    {
        var courses = Data.Levels.Where(l => l.IsPublished);
        if (!string.IsNullOrWhiteSpace(q))
            courses = courses.Where(l => l.NameEn.Contains(q, StringComparison.OrdinalIgnoreCase)
                || l.ShortDescriptionEn.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        if (category.HasValue)
            courses = courses.Where(l => l.Category == category.Value).ToList();

        return Ok(courses.OrderBy(l => l.Order).Select(ToDto));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public IActionResult Get(string id)
    {
        var level = Data.Levels.Find(id);
        if (level == null) return NotFound();

        var lessons = Data.Lessons.Where(x => x.LevelId == id).OrderBy(x => x.Order).Select(x => new
        {
            x.Id,
            x.Order,
            x.TitleEn,
            x.TitleKm,
            x.EstimatedMinutes,
            x.XpValue
        });

        return Ok(new { course = ToDto(level), lessons });
    }

    [HttpGet("{id}/lessons/{lessonId}")]
    [Authorize]
    public IActionResult GetLesson(string id, string lessonId)
    {
        var lesson = Data.Lessons.Find(lessonId);
        if (lesson == null || lesson.LevelId != id) return NotFound();

        var quizzes = Data.QuizItems.Where(q => q.LessonId == lessonId)
            .Select(q => new { q.Id, q.Type, q.QuestionKm, q.Options, q.Difficulty, q.XpValue });
        var challenges = Data.CodingChallenges.Where(c => c.LessonId == lessonId)
            .Select(c => new { c.Id, c.PromptKm, c.StarterCode, c.Difficulty, c.XpValue });

        return Ok(new { lesson, quizzes, challenges });
    }

    // ---- Admin CRUD ----

    [HttpPost("~/api/admin/courses")]
    [Authorize(Roles = "Admin")]
    public IActionResult Create(CourseSaveRequest req)
    {
        var level = new Level
        {
            Id = string.IsNullOrWhiteSpace(req.Id) ? Guid.NewGuid().ToString("N")[..8].ToUpper() : req.Id,
            Order = req.Order,
            NameEn = req.NameEn,
            NameKm = req.NameKm,
            ShortDescriptionEn = req.ShortDescriptionEn,
            DescriptionKm = req.DescriptionKm,
            Category = req.Category,
            ThumbnailEmoji = string.IsNullOrWhiteSpace(req.ThumbnailEmoji) ? "📘" : req.ThumbnailEmoji,
            AccentColor = string.IsNullOrWhiteSpace(req.AccentColor) ? "#6366f1" : req.AccentColor,
            Price = req.Price,
            IsPublished = req.IsPublished,
            EstimatedHours = req.EstimatedHours,
            Prerequisite = req.Prerequisite
        };
        Data.Levels.Upsert(level);
        return Ok(ToDto(level));
    }

    [HttpPut("~/api/admin/courses/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(string id, CourseSaveRequest req)
    {
        var level = Data.Levels.Find(id);
        if (level == null) return NotFound();

        level.Order = req.Order;
        level.NameEn = req.NameEn;
        level.NameKm = req.NameKm;
        level.ShortDescriptionEn = req.ShortDescriptionEn;
        level.DescriptionKm = req.DescriptionKm;
        level.Category = req.Category;
        level.ThumbnailEmoji = req.ThumbnailEmoji;
        level.AccentColor = req.AccentColor;
        level.Price = req.Price;
        level.IsPublished = req.IsPublished;
        level.EstimatedHours = req.EstimatedHours;
        level.Prerequisite = req.Prerequisite;
        Data.Levels.Upsert(level);
        return Ok(ToDto(level));
    }

    [HttpDelete("~/api/admin/courses/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(string id)
    {
        if (Data.ClassSections.Where(c => c.LevelId == id).Any())
            return BadRequest(new { message = "Can't delete a course that still has classes scheduled against it." });
        Data.Levels.Delete(id);
        return Ok();
    }

    [HttpGet("~/api/admin/courses")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminList()
        => Ok(Data.Levels.All().OrderBy(l => l.Order).Select(ToDto));

    // ---- Admin: lesson / content management ----

    [HttpGet("~/api/admin/courses/{courseId}/lessons")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminLessons(string courseId)
        => Ok(Data.Lessons.Where(l => l.LevelId == courseId).OrderBy(l => l.Order).Select(LessonAdminDto.From));

    [HttpPost("~/api/admin/lessons")]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateLesson(LessonSaveRequest req)
    {
        var lesson = new Lesson
        {
            LevelId = req.LevelId,
            Order = req.Order,
            TitleEn = req.TitleEn,
            TitleKm = req.TitleKm,
            EstimatedMinutes = req.EstimatedMinutes,
            XpValue = req.XpValue,
            VideoUrl = req.VideoUrl,
            DurationSeconds = req.DurationSeconds,
            ExplainerMd = req.ExplainerMd,
            ResourcesMd = req.ResourcesMd
        };
        Data.Lessons.Upsert(lesson);
        return Ok(LessonAdminDto.From(lesson));
    }

    [HttpPut("~/api/admin/lessons/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateLesson(string id, LessonSaveRequest req)
    {
        var lesson = Data.Lessons.Find(id);
        if (lesson == null) return NotFound();
        lesson.Order = req.Order;
        lesson.TitleEn = req.TitleEn;
        lesson.TitleKm = req.TitleKm;
        lesson.EstimatedMinutes = req.EstimatedMinutes;
        lesson.XpValue = req.XpValue;
        lesson.VideoUrl = req.VideoUrl;
        lesson.DurationSeconds = req.DurationSeconds;
        lesson.ExplainerMd = req.ExplainerMd;
        lesson.ResourcesMd = req.ResourcesMd;
        Data.Lessons.Upsert(lesson);
        return Ok(LessonAdminDto.From(lesson));
    }

    [HttpDelete("~/api/admin/lessons/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteLesson(string id)
    {
        Data.Lessons.Delete(id);
        return Ok();
    }
}
