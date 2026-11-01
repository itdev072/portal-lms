using Portal.Web.Models.Domain;

namespace Portal.Web.Models.ViewModels;

public class LevelFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public int Order { get; set; } = 1;
    public string NameKm { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string SourceChaptersRaw { get; set; } = string.Empty;
    public string? Prerequisite { get; set; }
    public int EstimatedHours { get; set; } = 12;
    public string DescriptionKm { get; set; } = string.Empty;

    public static LevelFormViewModel FromDomain(Level l) => new()
    {
        Id = l.Id,
        Order = l.Order,
        NameKm = l.NameKm,
        NameEn = l.NameEn,
        SourceChaptersRaw = string.Join(",", l.SourceChapters),
        Prerequisite = l.Prerequisite,
        EstimatedHours = l.EstimatedHours,
        DescriptionKm = l.DescriptionKm
    };

    public Level ToDomain() => new()
    {
        Id = Id,
        Order = Order,
        NameKm = NameKm,
        NameEn = NameEn,
        SourceChapters = SourceChaptersRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : (int?)null)
            .Where(n => n.HasValue).Select(n => n!.Value).ToList(),
        Prerequisite = string.IsNullOrWhiteSpace(Prerequisite) ? null : Prerequisite,
        EstimatedHours = EstimatedHours,
        DescriptionKm = DescriptionKm
    };
}
