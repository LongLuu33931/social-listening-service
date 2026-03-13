using Coka.Social.Listening.Core.Attributes;

namespace Coka.Social.Listening.Core.Entities;

[Table("provinces")]
public class ProvinceEntity : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Column("center")]
    public string? Center { get; set; }

    [Column("region")]
    public string Region { get; set; } = string.Empty;

    [Column("merged_from")]
    public string[]? MergedFrom { get; set; }

    [Column("natural_area_km2")]
    public decimal? NaturalAreaKm2 { get; set; }

    [Column("population_thousands")]
    public decimal? PopulationThousands { get; set; }

    [Column("coords")]
    public string? Coords { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;
}
