namespace Coka.Social.Listening.Core.DTOs;

// ── TikTok mention ────────────────────────────────────────────────────
public class TiktokMentionDto
{
    public int Id { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string? AuthorNickname { get; set; }
    public string? AuthorUniqueId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Duration { get; set; }
    public string? CoverUrl { get; set; }
    public string? VideoUrl { get; set; }
    public long? CreateTime { get; set; }
    public bool IsApproved { get; set; }
}

// ── YouTube mention ───────────────────────────────────────────────────
public class YoutubeMentionDto
{
    public int Id { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string? ChannelId { get; set; }
    public string? ChannelTitle { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public long CommentCount { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string[]? Tags { get; set; }
    public bool IsApproved { get; set; }
}
