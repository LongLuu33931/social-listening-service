using Coka.Social.Listening.Core.Attributes;

namespace Coka.Social.Listening.Core.Entities;

public abstract class BaseEntity
{
    [PrimaryKey]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("updated_by")]
    public string? UpdatedBy { get; set; }
}
