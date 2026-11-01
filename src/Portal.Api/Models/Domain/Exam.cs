namespace Portal.Api.Models.Domain;

/// <summary>
/// A formal, timed, single-attempt assessment — distinct from a lesson's quiz (ungraded
/// practice). A midterm belongs to one class section and is built from ExamQuestions by
/// the teacher.
/// </summary>
public class MidtermExam
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ClassSectionId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LevelId { get; set; } = string.Empty; // the CSK level this exam covers
    public int TimeLimitMinutes { get; set; } = 60;
    public DateTimeOffset OpensAt { get; set; }
    public DateTimeOffset ClosesAt { get; set; }
    public bool IsPublished { get; set; }
}

public class ExamQuestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ExamId { get; set; } = string.Empty;
    public int Order { get; set; }
    public QuizType Type { get; set; } = QuizType.MultipleChoice;
    public string QuestionKm { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
    public int Points { get; set; } = 10;
}

public class ExamAnswer
{
    public string QuestionId { get; set; } = string.Empty;
    public int SelectedIndex { get; set; } = -1;
}

public class ExamAttempt
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ExamId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SubmittedAt { get; set; }
    public List<ExamAnswer> Answers { get; set; } = new();
    public int Score { get; set; }
    public int MaxScore { get; set; }
}
