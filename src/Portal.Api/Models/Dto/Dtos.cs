using Portal.Api.Models.Domain;

namespace Portal.Api.Models.Dto;

public record UserDto(string Id, string Email, string DisplayName, UserRole Role, bool IsActive, DateTimeOffset CreatedAt)
{
    public static UserDto From(ApplicationUser u) => new(u.Id, u.Email, u.DisplayName, u.Role, u.IsActive, u.CreatedAt);
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

public record CreateUserRequest(string DisplayName, string Email, string Password, UserRole Role);
public record UpdateUserRequest(string DisplayName, UserRole Role, bool IsActive);

public record MarkAttendanceRequest(string StudentId, AttendanceStatus Status, DateOnly? Date);

public record AttendanceRosterEntry(string StudentId, string StudentName, AttendanceStatus? Status, string Source);
