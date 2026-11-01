namespace Portal.Web.Models.Domain;

public class ClassSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string SemesterLabel { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Enrollment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string ClassSectionId { get; set; } = string.Empty;
    public DateTimeOffset EnrolledAt { get; set; } = DateTimeOffset.UtcNow;
}
