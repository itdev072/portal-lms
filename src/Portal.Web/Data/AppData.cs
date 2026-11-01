using Portal.Web.Models.Domain;

namespace Portal.Web.Data;

/// <summary>
/// The single entry point to every collection in the Portal. Registered as a singleton
/// in Program.cs. Controllers and services take an AppData dependency rather than
/// individual JsonCollection&lt;T&gt; instances.
/// </summary>
public class AppData
{
    public JsonCollection<ApplicationUser> Users { get; }
    public JsonCollection<ClassSection> ClassSections { get; }
    public JsonCollection<Enrollment> Enrollments { get; }
    public JsonCollection<Level> Levels { get; }
    public JsonCollection<Lesson> Lessons { get; }
    public JsonCollection<QuizItem> QuizItems { get; }
    public JsonCollection<CodingChallenge> CodingChallenges { get; }
    public JsonCollection<CaseStudyTask> CaseStudyTasks { get; }
    public JsonCollection<StudentProgress> Progress { get; }
    public JsonCollection<Attempt> Attempts { get; }
    public JsonCollection<MidtermExam> Exams { get; }
    public JsonCollection<ExamQuestion> ExamQuestions { get; }
    public JsonCollection<ExamAttempt> ExamAttempts { get; }
    public JsonCollection<CampusLocation> CampusLocations { get; }
    public JsonCollection<AttendanceSession> AttendanceSessions { get; }
    public JsonCollection<AttendanceRecord> AttendanceRecords { get; }

    public AppData(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "App_Data");

        Users = new JsonCollection<ApplicationUser>(dataDir, "users.json", u => u.Id);
        ClassSections = new JsonCollection<ClassSection>(dataDir, "class_sections.json", c => c.Id);
        Enrollments = new JsonCollection<Enrollment>(dataDir, "enrollments.json", e => e.Id);
        Levels = new JsonCollection<Level>(dataDir, "levels.json", l => l.Id);
        Lessons = new JsonCollection<Lesson>(dataDir, "lessons.json", l => l.Id);
        QuizItems = new JsonCollection<QuizItem>(dataDir, "quiz_items.json", q => q.Id);
        CodingChallenges = new JsonCollection<CodingChallenge>(dataDir, "coding_challenges.json", c => c.Id);
        CaseStudyTasks = new JsonCollection<CaseStudyTask>(dataDir, "case_study_tasks.json", c => c.Id);
        Progress = new JsonCollection<StudentProgress>(dataDir, "progress.json", p => p.Id);
        Attempts = new JsonCollection<Attempt>(dataDir, "attempts.json", a => a.Id);
        Exams = new JsonCollection<MidtermExam>(dataDir, "exams.json", e => e.Id);
        ExamQuestions = new JsonCollection<ExamQuestion>(dataDir, "exam_questions.json", q => q.Id);
        ExamAttempts = new JsonCollection<ExamAttempt>(dataDir, "exam_attempts.json", a => a.Id);
        CampusLocations = new JsonCollection<CampusLocation>(dataDir, "campus_locations.json", c => c.Id);
        AttendanceSessions = new JsonCollection<AttendanceSession>(dataDir, "attendance_sessions.json", s => s.Id);
        AttendanceRecords = new JsonCollection<AttendanceRecord>(dataDir, "attendance_records.json", r => r.Id);
    }
}
