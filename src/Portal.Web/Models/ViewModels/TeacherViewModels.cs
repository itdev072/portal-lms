using Portal.Web.Models.Domain;

namespace Portal.Web.Models.ViewModels;

public class TeacherDashboardViewModel
{
    public List<ClassSection> Sections { get; set; } = new();
    public List<MidtermExam> Exams { get; set; } = new();
    public List<AttendanceSession> RecentSessions { get; set; } = new();
    public List<Level> Levels { get; set; } = new();
}

public class StudentRosterRow
{
    public ApplicationUser Student { get; set; } = null!;
    public int TotalXp { get; set; }
    public int LevelsCompleted { get; set; }
    public int LessonsCompleted { get; set; }
}

public class RosterViewModel
{
    public ClassSection Section { get; set; } = null!;
    public List<StudentRosterRow> Students { get; set; } = new();
}

public class ContentListViewModel
{
    public Level Level { get; set; } = null!;
    public List<Lesson> Lessons { get; set; } = new();
    public List<QuizItem> QuizItems { get; set; } = new();
    public List<CodingChallenge> Challenges { get; set; } = new();
}

public class CreateAttendanceSessionViewModel
{
    public string ClassSectionId { get; set; } = string.Empty;
    public string CampusLocationId { get; set; } = string.Empty;
    public int ValidMinutes { get; set; } = 10;
    public List<ClassSection> Sections { get; set; } = new();
    public List<CampusLocation> Campuses { get; set; } = new();
}

public class AttendanceSessionViewModel
{
    public AttendanceSession Session { get; set; } = null!;
    public ClassSection Section { get; set; } = null!;
    public CampusLocation Campus { get; set; } = null!;
    public List<AttendanceRecordRow> Records { get; set; } = new();
    public string CheckInUrl { get; set; } = string.Empty;
}

public class AttendanceRecordRow
{
    public AttendanceRecord Record { get; set; } = null!;
    public string StudentName { get; set; } = string.Empty;
}

public class CreateExamViewModel
{
    public string ClassSectionId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LevelId { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; } = 60;
    public DateTimeOffset OpensAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ClosesAt { get; set; } = DateTimeOffset.UtcNow.AddDays(7);
    public List<ClassSection> Sections { get; set; } = new();
    public List<Level> Levels { get; set; } = new();
}

public class AddExamQuestionViewModel
{
    public string ExamId { get; set; } = string.Empty;
    public string QuestionKm { get; set; } = string.Empty;
    public string Option1 { get; set; } = string.Empty;
    public string Option2 { get; set; } = string.Empty;
    public string Option3 { get; set; } = string.Empty;
    public string Option4 { get; set; } = string.Empty;
    public int CorrectIndex { get; set; }
    public int Points { get; set; } = 10;
}

public class ExamManageViewModel
{
    public MidtermExam Exam { get; set; } = null!;
    public List<ExamQuestion> Questions { get; set; } = new();
}

public class ExamAttemptRow
{
    public ApplicationUser Student { get; set; } = null!;
    public ExamAttempt? Attempt { get; set; }
}

public class ExamResultsViewModel
{
    public MidtermExam Exam { get; set; } = null!;
    public List<ExamAttemptRow> Rows { get; set; } = new();
}
