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

// ── Bulk create DTOs ─────────────────────────────────────────────────
public class CreateTiktokVideoDto
{
    public Guid? ProjectId { get; set; }
    public string? KeywordSearch { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string? AwemeId { get; set; }
    public string? AuthorId { get; set; }
    public string? AuthorNickname { get; set; }
    public string? AuthorUniqueId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public long? ShareCount { get; set; }
    public long? CommentCount { get; set; }
    public long? DownloadCount { get; set; }
    public string? Duration { get; set; }
    public string? CreateTime { get; set; }
    public string? VideoUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? PlayUrl { get; set; }
    public string? ChannelId { get; set; }
    public string? ChannelTitle { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string[]? Tags { get; set; }
}

public class CreateYoutubeVideoDto
{
    public Guid? ProjectId { get; set; }
    public string? KeywordSearch { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string? AwemeId { get; set; }
    public string? AuthorId { get; set; }
    public string? AuthorNickname { get; set; }
    public string? AuthorUniqueId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long? ShareCount { get; set; }
    public long? CommentCount { get; set; }
    public long? DownloadCount { get; set; }
    public string? Duration { get; set; }
    public string? CreateTime { get; set; }
    public string? VideoUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? PlayUrl { get; set; }
    public string? ChannelId { get; set; }
    public string? ChannelTitle { get; set; }
    public DateTime? PublishedAt { get; set; }
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string[]? Tags { get; set; }
}

public class BulkCreateVideoResultDto
{
    public int Inserted { get; set; }
    public int Skipped { get; set; }
}

// ── Facebook mention ─────────────────────────────────────────────────
public class FacebookPostMentionDto
{
    public int Id { get; set; }
    public string PostId { get; set; } = string.Empty;
    public string? PostUrl { get; set; }
    public string? PostType { get; set; }
    public string? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorUrl { get; set; }
    public string? Message { get; set; }
    public long ReactionCount { get; set; }
    public long CommentCount { get; set; }
    public long ShareCount { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsApproved { get; set; }
}

public class CreateFacebookPostDto
{
    public Guid? ProjectId { get; set; }
    public string? KeywordSearch { get; set; }
    public string PostId { get; set; } = string.Empty;
    public string? PostUrl { get; set; }
    public string? PostType { get; set; }
    public string? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorUrl { get; set; }
    public string? AuthorAvatar { get; set; }
    public string? Message { get; set; }
    public long? ReactionCount { get; set; }
    public long? LikeCount { get; set; }
    public long? LoveCount { get; set; }
    public long? HahaCount { get; set; }
    public long? WowCount { get; set; }
    public long? SadCount { get; set; }
    public long? AngryCount { get; set; }
    public long? CommentCount { get; set; }
    public long? ShareCount { get; set; }
    public string[]? ImageUrls { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? SharedLink { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? RawJson { get; set; }
}

public class FacebookCachedPostDto
{
    public string PostId { get; set; } = string.Empty;
    public string? PostUrl { get; set; }
}
