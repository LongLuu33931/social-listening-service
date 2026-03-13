using Coka.Social.Listening.Core.Attributes;

namespace Coka.Social.Listening.Core.Entities;

[Table("articles")]
public class ArticleEntity : BaseEntity
{
    [Column("project_id")]
    public Guid? ProjectId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("url")]
    public string Url { get; set; } = string.Empty;

    [Column("source_name")]
    public string? SourceName { get; set; }

    [Column("source_url")]
    public string? SourceUrl { get; set; }

    [Column("pub_date")]
    public DateTime? PubDate { get; set; }

    [Column("snippet")]
    public string? Snippet { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("sentiment")]
    public string? Sentiment { get; set; }

    [Column("is_relevant")]
    public bool IsRelevant { get; set; } = true;

    [Column("status")]
    public int Status { get; set; } = 2;
}
