using Coka.Social.Listening.Core.Attributes;

namespace Coka.Social.Listening.Core.Entities;

[Table("categories")]
public class CategoryEntity : BaseEntity
{
    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("label")]
    public string Label { get; set; } = string.Empty;

    [Column("group_key")]
    public string? GroupKey { get; set; }

    [Column("group_label")]
    public string? GroupLabel { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;
}
