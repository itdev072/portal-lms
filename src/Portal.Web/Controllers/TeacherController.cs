using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Web.Data;
using Portal.Web.Models.Domain;
using Portal.Web.Models.ViewModels;
using Portal.Web.Services;

namespace Portal.Web.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly AppData _data;
    private readonly AttendanceService _attendance;

    public TeacherController(AppData data, AttendanceService attendance)
    {
        _data = data;
        _attendance = attendance;
    }

    private string TeacherId => User.GetUserId()!;

    public IActionResult Dashboard()
    {
        var sections = _data.ClassSections.Where(s => s.TeacherId == TeacherId).ToList();
        var sectionIds = sections.Select(s => s.Id).ToHashSet();

        return View(new TeacherDashboardViewModel
        {
            Sections = sections,
            Exams = _data.Exams.Where(e => sectionIds.Contains(e.ClassSectionId)).ToList(),
            RecentSessions = _data.AttendanceSessions.Where(s => s.TeacherId == TeacherId)
                .OrderByDescending(s => s.CreatedAt).Take(5).ToList(),
            Levels = _data.Levels.All().OrderBy(l => l.Order).ToList()
        });
    }

    public IActionResult Roster(string sectionId)
    {
        var section = _data.ClassSections.Find(sectionId);
        if (section == null || section.TeacherId != TeacherId) return NotFound();

        var studentIds = _data.Enrollments.Where(e => e.ClassSectionId == sectionId).Select(e => e.StudentId).ToList();
        var rows = studentIds.Select(sid =>
        {
            var student = _data.Users.Find(sid);
            var progress = _data.Progress.Where(p => p.StudentId == sid);
            return new StudentRosterRow
            {
                Student = student!,
                TotalXp = progress.Where(p => p.LessonId == null).Sum(p => p.XpEarned),
                LevelsCompleted = progress.Count(p => p.LessonId == null && p.Status == ProgressStatus.Completed),
                LessonsCompleted = progress.Count(p => p.LessonId != null && p.Status == ProgressStatus.Completed)
            };
        }).Where(r => r.Student != null).ToList();

        return View(new RosterViewModel { Section = section, Students = rows });
    }

    // ---------- Content management ----------

    public new IActionResult Content(string levelId)
    {
        var level = _data.Levels.Find(levelId);
        if (level == null) return NotFound();

        return View(new ContentListViewModel
        {
            Level = level,
            Lessons = _data.Lessons.Where(l => l.LevelId == levelId).OrderBy(l => l.Order).ToList(),
            QuizItems = _data.QuizItems.Where(q => q.LevelId == levelId).ToList(),
            Challenges = _data.CodingChallenges.Where(c => c.LevelId == levelId).ToList()
        });
    }

    [HttpGet]
    public IActionResult CreateLesson(string levelId) => View("EditLesson", new LessonFormViewModel { LevelId = levelId });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateLesson(LessonFormViewModel model)
    {
        model.Id = "";
        var lesson = model.ToDomain();
        _data.Lessons.Upsert(lesson);
        return RedirectToAction(nameof(Content), new { levelId = lesson.LevelId });
    }

    [HttpGet]
    public IActionResult EditLesson(string id)
    {
        var lesson = _data.Lessons.Find(id);
        return lesson == null ? NotFound() : View(LessonFormViewModel.FromDomain(lesson));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult EditLesson(LessonFormViewModel model)
    {
        var lesson = model.ToDomain();
        _data.Lessons.Upsert(lesson);
        return RedirectToAction(nameof(Content), new { levelId = lesson.LevelId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteLesson(string id, string levelId)
    {
        _data.Lessons.Delete(id);
        return RedirectToAction(nameof(Content), new { levelId });
    }

    [HttpGet]
    public IActionResult CreateQuiz(string levelId, string lessonId) => View(new QuizItem { LevelId = levelId, LessonId = lessonId, Options = new List<string> { "", "", "", "" } });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateQuiz(QuizItem model)
    {
        model.Id = Guid.NewGuid().ToString("N");
        model.Options = model.Options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
        _data.QuizItems.Upsert(model);
        return RedirectToAction(nameof(Content), new { levelId = model.LevelId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteQuiz(string id, string levelId)
    {
        _data.QuizItems.Delete(id);
        return RedirectToAction(nameof(Content), new { levelId });
    }

    [HttpGet]
    public IActionResult CreateChallenge(string levelId, string lessonId) => View(new CodingChallenge { LevelId = levelId, LessonId = lessonId, TestCases = new List<TestCase> { new() } });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateChallenge(CodingChallenge model)
    {
        model.Id = Guid.NewGuid().ToString("N");
        _data.CodingChallenges.Upsert(model);
        return RedirectToAction(nameof(Content), new { levelId = model.LevelId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteChallenge(string id, string levelId)
    {
        _data.CodingChallenges.Delete(id);
        return RedirectToAction(nameof(Content), new { levelId });
    }

    // ---------- Attendance ----------

    [HttpGet]
    public IActionResult CreateAttendanceSession()
    {
        return View(new CreateAttendanceSessionViewModel
        {
            Sections = _data.ClassSections.Where(s => s.TeacherId == TeacherId).ToList(),
            Campuses = _data.CampusLocations.All()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateAttendanceSession(CreateAttendanceSessionViewModel model)
    {
        var session = _attendance.CreateSession(model.ClassSectionId, TeacherId, model.CampusLocationId, model.ValidMinutes);
        return RedirectToAction(nameof(AttendanceSession), new { id = session.Id });
    }

    public IActionResult AttendanceSession(string id)
    {
        var session = _data.AttendanceSessions.Find(id);
        if (session == null || session.TeacherId != TeacherId) return NotFound();

        var section = _data.ClassSections.Find(session.ClassSectionId)!;
        var campus = _data.CampusLocations.Find(session.CampusLocationId)!;
        var records = _data.AttendanceRecords.Where(r => r.SessionId == id)
            .OrderByDescending(r => r.ScannedAt)
            .Select(r => new AttendanceRecordRow { Record = r, StudentName = _data.Users.Find(r.StudentId)?.DisplayName ?? "Unknown" })
            .ToList();

        var checkInUrl = Url.Action("CheckIn", "Attendance", new { token = session.Token }, Request.Scheme)!;

        return View(new AttendanceSessionViewModel
        {
            Session = session,
            Section = section,
            Campus = campus,
            Records = records,
            CheckInUrl = checkInUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CloseAttendanceSession(string id)
    {
        var session = _data.AttendanceSessions.Find(id);
        if (session == null || session.TeacherId != TeacherId) return NotFound();
        session.IsClosed = true;
        _data.AttendanceSessions.Upsert(session);
        return RedirectToAction(nameof(AttendanceSession), new { id });
    }

    // ---------- Midterm exams ----------

    [HttpGet]
    public IActionResult CreateExam()
    {
        return View(new CreateExamViewModel
        {
            Sections = _data.ClassSections.Where(s => s.TeacherId == TeacherId).ToList(),
            Levels = _data.Levels.All().OrderBy(l => l.Order).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateExam(CreateExamViewModel model)
    {
        var exam = new MidtermExam
        {
            ClassSectionId = model.ClassSectionId,
            Title = model.Title,
            LevelId = model.LevelId,
            TimeLimitMinutes = model.TimeLimitMinutes,
            OpensAt = model.OpensAt,
            ClosesAt = model.ClosesAt,
            IsPublished = false
        };
        _data.Exams.Upsert(exam);
        return RedirectToAction(nameof(ManageExam), new { id = exam.Id });
    }

    public IActionResult ManageExam(string id)
    {
        var exam = _data.Exams.Find(id);
        if (exam == null) return NotFound();
        return View(new ExamManageViewModel
        {
            Exam = exam,
            Questions = _data.ExamQuestions.Where(q => q.ExamId == id).OrderBy(q => q.Order).ToList()
        });
    }

    [HttpGet]
    public IActionResult AddExamQuestion(string examId) => View(new AddExamQuestionViewModel { ExamId = examId });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AddExamQuestion(AddExamQuestionViewModel model)
    {
        var order = _data.ExamQuestions.Where(q => q.ExamId == model.ExamId).Count() + 1;
        _data.ExamQuestions.Upsert(new ExamQuestion
        {
            ExamId = model.ExamId,
            Order = order,
            QuestionKm = model.QuestionKm,
            Options = new List<string> { model.Option1, model.Option2, model.Option3, model.Option4 }
                .Where(o => !string.IsNullOrWhiteSpace(o)).ToList(),
            CorrectIndex = model.CorrectIndex,
            Points = model.Points
        });
        return RedirectToAction(nameof(ManageExam), new { id = model.ExamId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult PublishExam(string id)
    {
        var exam = _data.Exams.Find(id);
        if (exam == null) return NotFound();
        exam.IsPublished = true;
        _data.Exams.Upsert(exam);
        return RedirectToAction(nameof(ManageExam), new { id });
    }

    public IActionResult ExamResults(string id)
    {
        var exam = _data.Exams.Find(id);
        if (exam == null) return NotFound();

        var studentIds = _data.Enrollments.Where(e => e.ClassSectionId == exam.ClassSectionId).Select(e => e.StudentId);
        var rows = studentIds.Select(sid => new ExamAttemptRow
        {
            Student = _data.Users.Find(sid)!,
            Attempt = _data.ExamAttempts.Where(a => a.ExamId == id && a.StudentId == sid).FirstOrDefault()
        }).Where(r => r.Student != null).ToList();

        return View(new ExamResultsViewModel { Exam = exam, Rows = rows });
    }
}
