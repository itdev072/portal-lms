namespace Portal.Web.Models.Domain;

/// <summary>
/// One CSK level. Mirrors the "level" object in the Phase 1 content master prompt's
/// JSON schema (Master_Prompt_CSharp_Learning_App_Content.md) so that JSON produced
/// from that prompt can be imported straight into this shape.
/// </summary>
public class Level
{
    public string Id { get; set; } = string.Empty; // e.g. "CSK1"
    public int Order { get; set; }
    public string NameKm { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public List<int> SourceChapters { get; set; } = new();
    public string? Prerequisite { get; set; } // another Level.Id, or null for the first level
    public int EstimatedHours { get; set; }
    public string DescriptionKm { get; set; } = string.Empty;
}

public class KeyTerm
{
    public string Km { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
}

public class CodeExample
{
    public string Caption { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class Lesson
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string LevelId { get; set; } = string.Empty;
    public int Order { get; set; }
    public string TitleKm { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public List<KeyTerm> KeyTerms { get; set; } = new();
    public string ExplainerMd { get; set; } = string.Empty;
    public List<CodeExample> CodeExamples { get; set; } = new();
    public int XpValue { get; set; } = 10;
    public int EstimatedMinutes { get; set; } = 15;
}

public class QuizItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string LevelId { get; set; } = string.Empty;
    public string LessonId { get; set; } = string.Empty;
    public QuizType Type { get; set; } = QuizType.MultipleChoice;
    public string QuestionKm { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
    public string ExplanationKm { get; set; } = string.Empty;
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    public int XpValue { get; set; } = 5;
}

public class TestCase
{
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
}

public class CodingChallenge
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string LevelId { get; set; } = string.Empty;
    public string LessonId { get; set; } = string.Empty;
    public string PromptKm { get; set; } = string.Empty;
    public string StarterCode { get; set; } = string.Empty;
    public string SolutionCode { get; set; } = string.Empty;
    public List<TestCase> TestCases { get; set; } = new();
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    public int XpValue { get; set; } = 15;
}

public class CaseStudyTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string LevelId { get; set; } = string.Empty;
    public string PromptKm { get; set; } = string.Empty;
    public string SolutionNotesKm { get; set; } = string.Empty;
    public int XpValue { get; set; } = 20;
}
