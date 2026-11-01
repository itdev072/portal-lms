namespace Portal.Api.Models.Domain;

public class StudentProgress
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string LevelId { get; set; } = string.Empty;
    public string? LessonId { get; set; }
    public ProgressStatus Status { get; set; } = ProgressStatus.NotStarted;
    public int XpEarned { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class Attempt
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty; // QuizItem.Id or CodingChallenge.Id
    public AttemptItemType ItemType { get; set; }
    public string SubmittedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int XpAwarded { get; set; }
    public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
}
