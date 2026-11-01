using Portal.Api.Models.Domain;

namespace Portal.Api.Models.Dto;

public record UserDto(string Id, string Email, string DisplayName, UserRole Role, bool IsActive, DateTimeOffset CreatedAt, string? ProfilePictureDataUrl)
{
    public static UserDto From(ApplicationUser u) => new(u.Id, u.Email, u.DisplayName, u.Role, u.IsActive, u.CreatedAt, u.ProfilePictureDataUrl);
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string DisplayName, string Email, string Password);

public record CourseDto(
    string Id, int Order, string NameEn, string NameKm, string ShortDescriptionEn, string DescriptionKm,
    CourseCategory Category, string ThumbnailEmoji, string AccentColor, decimal Price, bool IsPublished,
    double RatingAverage, int RatingCount, int EstimatedHours, string? Prerequisite, int LessonCount, int EnrolledCount)
{
    public static CourseDto From(Level l, int lessonCount, int enrolledCount) => new(
        l.Id, l.Order, l.NameEn, l.NameKm, l.ShortDescriptionEn, l.DescriptionKm, l.Category, l.ThumbnailEmoji,
        l.AccentColor, l.Price, l.IsPublished, l.RatingAverage, l.RatingCount, l.EstimatedHours, l.Prerequisite,
        lessonCount, enrolledCount);
}

public record CourseSaveRequest(
    string Id, int Order, string NameEn, string NameKm, string ShortDescriptionEn, string DescriptionKm,
    CourseCategory Category, string ThumbnailEmoji, string AccentColor, decimal Price, bool IsPublished,
    int EstimatedHours, string? Prerequisite);

public record ShiftDto(string Id, string Name, TimeSpan StartTime, TimeSpan EndTime, List<DayOfWeek> Days)
{
    public static ShiftDto From(Shift s) => new(s.Id, s.Name, s.StartTime, s.EndTime, s.Days);
}
public record ShiftSaveRequest(string? Id, string Name, TimeSpan StartTime, TimeSpan EndTime, List<DayOfWeek> Days);

public record LocationDto(string Id, string Name, double Latitude, double Longitude, int RadiusMeters)
{
    public static LocationDto From(CampusLocation c) => new(c.Id, c.Name, c.Latitude, c.Longitude, c.RadiusMeters);
}
public record LocationSaveRequest(string? Id, string Name, double Latitude, double Longitude, int RadiusMeters);

public record ClassDto(
    string Id, string Name, string TeacherId, string TeacherName, string SemesterLabel,
    string LevelId, string CourseName, string ShiftId, string ShiftName, string CampusLocationId,
    string CampusLocationName, int Capacity, int EnrolledCount);

public record ClassSaveRequest(
    string? Id, string Name, string TeacherId, string SemesterLabel, string LevelId,
    string ShiftId, string CampusLocationId, int Capacity);

public record CreateUserRequest(string DisplayName, string Email, string Password, UserRole Role, string? ProfilePictureDataUrl);
public record UpdateUserRequest(string DisplayName, UserRole Role, bool IsActive, string? ProfilePictureDataUrl);

public record MarkAttendanceRequest(string StudentId, AttendanceStatus Status, DateOnly? Date);

public record AttendanceRosterEntry(string StudentId, string StudentName, AttendanceStatus? Status, string Source);

// ---- Lesson notes (course player, timestamped) ----

public record LessonNoteDto(string Id, string LessonId, double TimestampSeconds, string Text, DateTimeOffset CreatedAt)
{
    public static LessonNoteDto From(LessonNote n) => new(n.Id, n.LessonId, n.TimestampSeconds, n.Text, n.CreatedAt);
}
public record SaveNoteRequest(double TimestampSeconds, string Text);

// ---- Lesson Q&A ----

public record LessonQuestionDto(string Id, string LessonId, string AuthorId, string AuthorName, string Text, string? ParentId, DateTimeOffset CreatedAt)
{
    public static LessonQuestionDto From(LessonQuestion q) => new(q.Id, q.LessonId, q.AuthorId, q.AuthorName, q.Text, q.ParentId, q.CreatedAt);
}
public record AskQuestionRequest(string Text, string? ParentId);

// ---- Platform settings (admin) ----

public record SettingsDto(string SiteName, string SupportEmail, bool AllowPublicRegistration, int DefaultAttendanceRadiusMeters)
{
    public static SettingsDto From(PlatformSettings s) => new(s.SiteName, s.SupportEmail, s.AllowPublicRegistration, s.DefaultAttendanceRadiusMeters);
}
public record SettingsSaveRequest(string SiteName, string SupportEmail, bool AllowPublicRegistration, int DefaultAttendanceRadiusMeters);

// ---- Self-service account (any role: own profile picture + preferences) ----

public record AccountDto(string Id, string Email, string DisplayName, UserRole Role, string? ProfilePictureDataUrl, UserPreferences Preferences)
{
    public static AccountDto From(ApplicationUser u) => new(u.Id, u.Email, u.DisplayName, u.Role, u.ProfilePictureDataUrl, u.Preferences);
}
public record UpdateAccountRequest(string DisplayName, string? ProfilePictureDataUrl, UserPreferences Preferences);

// ---- Admin lesson/content management ----

public record LessonAdminDto(
    string Id, string LevelId, int Order, string TitleEn, string TitleKm, int EstimatedMinutes, int XpValue,
    string VideoUrl, int DurationSeconds, string ExplainerMd, string ResourcesMd)
{
    public static LessonAdminDto From(Lesson l) => new(
        l.Id, l.LevelId, l.Order, l.TitleEn, l.TitleKm, l.EstimatedMinutes, l.XpValue,
        l.VideoUrl, l.DurationSeconds, l.ExplainerMd, l.ResourcesMd);
}
public record LessonSaveRequest(
    string? Id, string LevelId, int Order, string TitleEn, string TitleKm, int EstimatedMinutes, int XpValue,
    string VideoUrl, int DurationSeconds, string ExplainerMd, string ResourcesMd);
