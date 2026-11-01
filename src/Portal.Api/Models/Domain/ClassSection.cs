namespace Portal.Api.Models.Domain;

public class ClassSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string SemesterLabel { get; set; } = string.Empty;
    public string LevelId { get; set; } = string.Empty; // the Course (Level) this class teaches
    public string ShiftId { get; set; } = string.Empty;
    public string CampusLocationId { get; set; } = string.Empty;
    public int Capacity { get; set; } = 30;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>A recurring time slot a class meets in, e.g. "Morning 8:00-10:00, Mon/Wed/Fri".</summary>
public class Shift
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; } = new(8, 0, 0);
    public TimeSpan EndTime { get; set; } = new(10, 0, 0);
    public List<DayOfWeek> Days { get; set; } = new();
}

public class Enrollment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string ClassSectionId { get; set; } = string.Empty;
    public DateTimeOffset EnrolledAt { get; set; } = DateTimeOffset.UtcNow;
}
