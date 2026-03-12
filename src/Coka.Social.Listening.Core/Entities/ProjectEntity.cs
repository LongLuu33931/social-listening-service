using Coka.Social.Listening.Core.Attributes;

namespace Coka.Social.Listening.Core.Entities;

[Table("projects")]
public class ProjectEntity : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("source")]
    public string? Source { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("investor")]
    public string? Investor { get; set; }

    [Column("origin_url")]
    public string? OriginUrl { get; set; }

    [Column("logo")]
    public string? Logo { get; set; }

    [Column("category_key")]
    public string? CategoryKey { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;

    [Column("confirmation_date")]
    public DateOnly? ConfirmationDate { get; set; }
}
