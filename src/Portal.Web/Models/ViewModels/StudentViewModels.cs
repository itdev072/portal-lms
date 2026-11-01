using Portal.Web.Models.Domain;

namespace Portal.Web.Models.ViewModels;

public class LevelSummary
{
    public Level Level { get; set; } = null!;
    public bool Unlocked { get; set; }
    public int TotalXp { get; set; }
    public int EarnedXp { get; set; }
    public int LessonCount { get; set; }
    public int CompletedLessonCount { get; set; }
}

public class StudentDashboardViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public int TotalXp { get; set; }
    public List<LevelSummary> Levels { get; set; } = new();
    public List<MidtermExam> OpenExams { get; set; } = new();
}

public class LevelDetailViewModel
{
    public Level Level { get; set; } = null!;
    public List<Lesson> Lessons { get; set; } = new();
    public HashSet<string> CompletedLessonIds { get; set; } = new();
    public CaseStudyTask? CaseStudyTask { get; set; }
}

public class LessonDetailViewModel
{
    public Lesson Lesson { get; set; } = null!;
    public Level Level { get; set; } = null!;
    public List<QuizItem> QuizItems { get; set; } = new();
    public CodingChallenge? Challenge { get; set; }
}

public class QuizResultViewModel
{
    public QuizItem Quiz { get; set; } = null!;
    public int SelectedIndex { get; set; }
    public bool IsCorrect { get; set; }
    public int XpAwarded { get; set; }
}

public class ChallengeResultViewModel
{
    public CodingChallenge Challenge { get; set; } = null!;
    public string SubmittedOutput { get; set; } = string.Empty;
    public bool AllPassed { get; set; }
    public List<bool> PerTestCase { get; set; } = new();
    public int XpAwarded { get; set; }
}

public class TakeExamViewModel
{
    public MidtermExam Exam { get; set; } = null!;
    public List<ExamQuestion> Questions { get; set; } = new();
}

public class ExamAnswerInput
{
    public string QuestionId { get; set; } = string.Empty;
    public int SelectedIndex { get; set; } = -1;
}

public class ExamResultViewModel
{
    public MidtermExam Exam { get; set; } = null!;
    public ExamAttempt Attempt { get; set; } = null!;
}

public class AttendanceCheckInViewModel
{
    public string Token { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public AttendanceStatus? Result { get; set; }
    public double? DistanceMeters { get; set; }
    public int? RadiusMeters { get; set; }
}
