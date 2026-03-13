namespace Coka.Social.Listening.Core.DTOs;

public class ProvinceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Center { get; set; }
    public string Region { get; set; } = string.Empty;
    public string[]? MergedFrom { get; set; }
    public decimal? NaturalAreaKm2 { get; set; }
    public decimal? PopulationThousands { get; set; }
    public string? Coords { get; set; }
}

public class ProvinceFilterDto
{
    public string? Search { get; set; }
    public string? Region { get; set; }   // 'bac' | 'trung' | 'nam'
}
