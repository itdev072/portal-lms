using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Web.Data;
using Portal.Web.Models.Domain;
using Portal.Web.Models.ViewModels;
using Portal.Web.Services;

namespace Portal.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly AppData _data;
    private readonly GradingService _grading;

    public StudentController(AppData data, GradingService grading)
    {
        _data = data;
        _grading = grading;
    }

    private string StudentId => User.GetUserId()!;

    public IActionResult Dashboard()
    {
        var levels = _data.Levels.All().OrderBy(l => l.Order).ToList();
        var allProgress = _data.Progress.Where(p => p.StudentId == StudentId);
        var completedLevelIds = allProgress.Where(p => p.LessonId == null && p.Status == ProgressStatus.Completed)
            .Select(p => p.LevelId).ToHashSet();

        var summaries = new List<LevelSummary>();
        foreach (var level in levels)
        {
            var unlocked = level.Prerequisite == null || completedLevelIds.Contains(level.Prerequisite);
            var lessons = _data.Lessons.Where(l => l.LevelId == level.Id);
            var lessonProgress = allProgress.Where(p => p.LevelId == level.Id && p.LessonId != null).ToList();
            var levelXpRow = allProgress.FirstOrDefault(p => p.LevelId == level.Id && p.LessonId == null);

            summaries.Add(new LevelSummary
            {
                Level = level,
                Unlocked = unlocked,
                LessonCount = lessons.Count,
                CompletedLessonCount = lessonProgress.Count(p => p.Status == ProgressStatus.Completed),
                EarnedXp = levelXpRow?.XpEarned ?? 0,
                TotalXp = lessons.Sum(l => l.XpValue)
                    + _data.QuizItems.Where(q => q.LevelId == level.Id).Sum(q => q.XpValue)
                    + _data.CodingChallenges.Where(c => c.LevelId == level.Id).Sum(c => c.XpValue)
            });
        }

        var mySections = _data.Enrollments.Where(e => e.StudentId == StudentId).Select(e => e.ClassSectionId).ToHashSet();
        var openExams = _data.Exams.Where(e => mySections.Contains(e.ClassSectionId) && e.IsPublished
            && DateTimeOffset.UtcNow >= e.OpensAt && DateTimeOffset.UtcNow <= e.ClosesAt).ToList();

        return View(new StudentDashboardViewModel
        {
            StudentName = User.GetDisplayName() ?? "",
            TotalXp = summaries.Sum(s => s.EarnedXp),
            Levels = summaries,
            OpenExams = openExams
        });
    }

    public IActionResult Level(string id)
    {
        var level = _data.Levels.Find(id);
        if (level == null) return NotFound();

        var lessons = _data.Lessons.Where(l => l.LevelId == id).OrderBy(l => l.Order).ToList();
        var completed = _data.Progress.Where(p => p.StudentId == StudentId && p.LevelId == id
            && p.LessonId != null && p.Status == ProgressStatus.Completed)
            .Select(p => p.LessonId!).ToHashSet();

        return View(new LevelDetailViewModel
        {
            Level = level,
            Lessons = lessons,
            CompletedLessonIds = completed,
            CaseStudyTask = _data.CaseStudyTasks.Where(c => c.LevelId == id).FirstOrDefault()
        });
    }

    public IActionResult Lesson(string id)
    {
        var lesson = _data.Lessons.Find(id);
        if (lesson == null) return NotFound();
        var level = _data.Levels.Find(lesson.LevelId)!;

        MarkLessonInProgress(lesson.Id, lesson.LevelId);

        return View(new LessonDetailViewModel
        {
            Lesson = lesson,
            Level = level,
            QuizItems = _data.QuizItems.Where(q => q.LessonId == id).ToList(),
            Challenge = _data.CodingChallenges.Where(c => c.LessonId == id).FirstOrDefault()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SubmitQuiz(string quizId, int selectedIndex)
    {
        var quiz = _data.QuizItems.Find(quizId);
        if (quiz == null) return NotFound();

        var attempt = _grading.GradeQuiz(StudentId, quiz, selectedIndex);
        if (attempt.IsCorrect) CompleteLessonIfDone(quiz.LessonId, quiz.LevelId);

        return View("QuizResult", new QuizResultViewModel
        {
            Quiz = quiz,
            SelectedIndex = selectedIndex,
            IsCorrect = attempt.IsCorrect,
            XpAwarded = attempt.XpAwarded
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SubmitChallenge(string challengeId, string submittedOutput)
    {
        var challenge = _data.CodingChallenges.Find(challengeId);
        if (challenge == null) return NotFound();

        var (allPassed, perTestCase) = _grading.GradeChallenge(StudentId, challenge, submittedOutput ?? "");
        if (allPassed) CompleteLessonIfDone(challenge.LessonId, challenge.LevelId);

        return View("ChallengeResult", new ChallengeResultViewModel
        {
            Challenge = challenge,
            SubmittedOutput = submittedOutput ?? "",
            AllPassed = allPassed,
            PerTestCase = perTestCase,
            XpAwarded = allPassed ? challenge.XpValue : 0
        });
    }

    public IActionResult Exams()
    {
        var mySections = _data.Enrollments.Where(e => e.StudentId == StudentId).Select(e => e.ClassSectionId).ToHashSet();
        var exams = _data.Exams.Where(e => mySections.Contains(e.ClassSectionId) && e.IsPublished).ToList();
        return View(exams);
    }

    public IActionResult TakeExam(string id)
    {
        var exam = _data.Exams.Find(id);
        if (exam == null) return NotFound();

        var now = DateTimeOffset.UtcNow;
        if (now < exam.OpensAt || now > exam.ClosesAt) return Forbid();

        var existingAttempt = _data.ExamAttempts.Where(a => a.ExamId == id && a.StudentId == StudentId).FirstOrDefault();
        if (existingAttempt is { SubmittedAt: not null })
            return RedirectToAction("ExamResult", new { id });

        var questions = _data.ExamQuestions.Where(q => q.ExamId == id).OrderBy(q => q.Order).ToList();
        return View(new TakeExamViewModel { Exam = exam, Questions = questions });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SubmitExam(string examId, List<ExamAnswerInput> answers)
    {
        var exam = _data.Exams.Find(examId);
        if (exam == null) return NotFound();

        var questions = _data.ExamQuestions.Where(q => q.ExamId == examId).ToList();

        var attempt = _data.ExamAttempts.Where(a => a.ExamId == examId && a.StudentId == StudentId).FirstOrDefault()
            ?? new ExamAttempt { ExamId = examId, StudentId = StudentId };

        attempt.Answers = answers.Select(a => new ExamAnswer { QuestionId = a.QuestionId, SelectedIndex = a.SelectedIndex }).ToList();

        _grading.GradeExam(attempt, questions);

        return RedirectToAction("ExamResult", new { id = examId });
    }

    public IActionResult ExamResult(string id)
    {
        var exam = _data.Exams.Find(id);
        var attempt = _data.ExamAttempts.Where(a => a.ExamId == id && a.StudentId == StudentId).FirstOrDefault();
        if (exam == null || attempt == null) return NotFound();

        return View(new ExamResultViewModel { Exam = exam, Attempt = attempt });
    }

    private void MarkLessonInProgress(string lessonId, string levelId)
    {
        var existing = _data.Progress.Where(p => p.StudentId == StudentId && p.LessonId == lessonId).FirstOrDefault();
        if (existing != null) return;

        _data.Progress.Upsert(new StudentProgress
        {
            StudentId = StudentId,
            LevelId = levelId,
            LessonId = lessonId,
            Status = ProgressStatus.InProgress
        });
    }

    private void CompleteLessonIfDone(string lessonId, string levelId)
    {
        var lessonQuizzes = _data.QuizItems.Where(q => q.LessonId == lessonId).Select(q => q.Id).ToList();
        var lessonChallenge = _data.CodingChallenges.Where(c => c.LessonId == lessonId).FirstOrDefault();

        var correctQuizIds = _data.Attempts.Where(a => a.StudentId == StudentId && a.IsCorrect
            && a.ItemType == AttemptItemType.Quiz && lessonQuizzes.Contains(a.ItemId))
            .Select(a => a.ItemId).ToHashSet();

        var challengeDone = lessonChallenge == null || _data.Attempts.Where(a => a.StudentId == StudentId
            && a.ItemType == AttemptItemType.Challenge && a.ItemId == lessonChallenge.Id && a.IsCorrect).Any();

        if (correctQuizIds.Count < lessonQuizzes.Count || !challengeDone) return;

        var progress = _data.Progress.Where(p => p.StudentId == StudentId && p.LessonId == lessonId).FirstOrDefault();
        if (progress == null) return;

        progress.Status = ProgressStatus.Completed;
        progress.CompletedAt = DateTimeOffset.UtcNow;
        _data.Progress.Upsert(progress);

        var allLessons = _data.Lessons.Where(l => l.LevelId == levelId).Select(l => l.Id).ToHashSet();
        var completedLessons = _data.Progress.Where(p => p.StudentId == StudentId && p.LevelId == levelId
            && p.LessonId != null && p.Status == ProgressStatus.Completed)
            .Select(p => p.LessonId!).ToHashSet();

        if (!allLessons.All(l => completedLessons.Contains(l))) return;

        var levelRow = _data.Progress.Where(p => p.StudentId == StudentId && p.LevelId == levelId && p.LessonId == null).FirstOrDefault()
            ?? new StudentProgress { StudentId = StudentId, LevelId = levelId, LessonId = null };

        levelRow.Status = ProgressStatus.Completed;
        levelRow.CompletedAt = DateTimeOffset.UtcNow;
        _data.Progress.Upsert(levelRow);
    }
}
