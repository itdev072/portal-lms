using Portal.Web.Models.Domain;

namespace Portal.Web.Models.ViewModels;

/// <summary>
/// A flattened form for creating/editing a Lesson. Domain Lesson has nested lists
/// (KeyTerms, CodeExamples) that aren't worth a dynamic multi-row JS form in this v1 —
/// one code example per lesson, key terms as a simple "km:en, km:en" line. Bulk content
/// (e.g. a full CSK level from the Phase 1 content master prompt's JSON) is a P1 import
/// feature, not hand-entry through this form.
/// </summary>
public class LessonFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public string LevelId { get; set; } = string.Empty;
    public int Order { get; set; } = 1;
    public string TitleKm { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string ExplainerMd { get; set; } = string.Empty;
    public string KeyTermsRaw { get; set; } = string.Empty;
    public string ExampleCaption { get; set; } = string.Empty;
    public string ExampleCode { get; set; } = string.Empty;
    public int XpValue { get; set; } = 10;
    public int EstimatedMinutes { get; set; } = 15;

    public static LessonFormViewModel FromDomain(Lesson l) => new()
    {
        Id = l.Id,
        LevelId = l.LevelId,
        Order = l.Order,
        TitleKm = l.TitleKm,
        TitleEn = l.TitleEn,
        ExplainerMd = l.ExplainerMd,
        KeyTermsRaw = string.Join(", ", l.KeyTerms.Select(k => $"{k.Km}:{k.En}")),
        ExampleCaption = l.CodeExamples.FirstOrDefault()?.Caption ?? "",
        ExampleCode = l.CodeExamples.FirstOrDefault()?.Code ?? "",
        XpValue = l.XpValue,
        EstimatedMinutes = l.EstimatedMinutes
    };

    public Lesson ToDomain()
    {
        var keyTerms = KeyTermsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(pair => pair.Split(':', 2))
            .Where(parts => parts.Length == 2)
            .Select(parts => new KeyTerm { Km = parts[0].Trim(), En = parts[1].Trim() })
            .ToList();

        var examples = new List<CodeExample>();
        if (!string.IsNullOrWhiteSpace(ExampleCode))
            examples.Add(new CodeExample { Caption = ExampleCaption, Code = ExampleCode });

        return new Lesson
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N") : Id,
            LevelId = LevelId,
            Order = Order,
            TitleKm = TitleKm,
            TitleEn = TitleEn,
            ExplainerMd = ExplainerMd,
            KeyTerms = keyTerms,
            CodeExamples = examples,
            XpValue = XpValue,
            EstimatedMinutes = EstimatedMinutes
        };
    }
}
