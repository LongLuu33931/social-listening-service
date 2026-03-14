using Dapper;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Services;
using Coka.Social.Listening.Infra.Data;
using Coka.Social.Listening.Infra.Helpers;

namespace Coka.Social.Listening.Infra.Services;

public class MentionService : IMentionService
{
    private readonly DbConnectionFactory _dbFactory;
    private readonly RedisHelper _redis;
    private static readonly TimeSpan FbCacheTtl = TimeSpan.FromHours(24);

    public MentionService(DbConnectionFactory dbFactory, RedisHelper redis)
    {
        _dbFactory = dbFactory;
        _redis = redis;
    }

    public async Task<bool> ApproveArticleAsync(Guid id, bool isApproved)
    {
        using var db = _dbFactory.CreateConnection();
        var sql = "UPDATE articles SET is_approved = @IsApproved, updated_at = NOW() WHERE id = @Id AND status = 1";
        var affected = await db.ExecuteAsync(sql, new { Id = id, IsApproved = isApproved });
        return affected > 0;
    }

    public async Task<bool> ApproveTiktokAsync(int id, bool isApproved)
    {
        using var db = _dbFactory.CreateConnection();
        var sql = "UPDATE tiktok_videos SET is_approved = @IsApproved WHERE id = @Id";
        var affected = await db.ExecuteAsync(sql, new { Id = id, IsApproved = isApproved });
        return affected > 0;
    }

    public async Task<bool> ApproveYoutubeAsync(int id, bool isApproved)
    {
        using var db = _dbFactory.CreateConnection();
        var sql = "UPDATE youtube_videos SET is_approved = @IsApproved WHERE id = @Id";
        var affected = await db.ExecuteAsync(sql, new { Id = id, IsApproved = isApproved });
        return affected > 0;
    }

    public async Task<bool> ApproveFacebookAsync(int id, bool isApproved)
    {
        using var db = _dbFactory.CreateConnection();
        var sql = "UPDATE facebook_posts SET is_approved = @IsApproved WHERE id = @Id";
        var affected = await db.ExecuteAsync(sql, new { Id = id, IsApproved = isApproved });
        return affected > 0;
    }

    public async Task<BulkApproveResultDto> BulkApproveAsync(BulkApproveMentionDto dto)
    {
        using var db = _dbFactory.CreateConnection();
        var result = new BulkApproveResultDto();

        if (dto.ArticleIds?.Count > 0)
        {
            var sql = "UPDATE articles SET is_approved = @IsApproved, updated_at = NOW() WHERE id = ANY(@Ids) AND status = 1";
            result.ArticlesUpdated = await db.ExecuteAsync(sql, new { Ids = dto.ArticleIds.ToArray(), IsApproved = dto.IsApproved });
        }

        if (dto.TiktokIds?.Count > 0)
        {
            var sql = "UPDATE tiktok_videos SET is_approved = @IsApproved WHERE id = ANY(@Ids)";
            result.TiktokUpdated = await db.ExecuteAsync(sql, new { Ids = dto.TiktokIds.ToArray(), IsApproved = dto.IsApproved });
        }

        if (dto.YoutubeIds?.Count > 0)
        {
            var sql = "UPDATE youtube_videos SET is_approved = @IsApproved WHERE id = ANY(@Ids)";
            result.YoutubeUpdated = await db.ExecuteAsync(sql, new { Ids = dto.YoutubeIds.ToArray(), IsApproved = dto.IsApproved });
        }

        if (dto.FacebookIds?.Count > 0)
        {
            var sql = "UPDATE facebook_posts SET is_approved = @IsApproved WHERE id = ANY(@Ids)";
            result.FacebookUpdated = await db.ExecuteAsync(sql, new { Ids = dto.FacebookIds.ToArray(), IsApproved = dto.IsApproved });
        }

        return result;
    }

    public async Task<BulkCreateVideoResultDto> BulkCreateTiktokAsync(List<CreateTiktokVideoDto> items)
    {
        if (items == null || items.Count == 0)
            return new BulkCreateVideoResultDto { Inserted = 0, Skipped = 0 };

        using var db = _dbFactory.CreateConnection();

        var sql = @"
            INSERT INTO tiktok_videos
                (project_id, keyword_search, video_id, aweme_id, author_id,
                 author_nickname, author_unique_id, title, description,
                 view_count, like_count, share_count, comment_count, download_count,
                 duration, create_time, video_url, cover_url, play_url,
                 channel_id, channel_title, published_at, thumbnail_url, tags)
            VALUES
                (@ProjectId, @KeywordSearch, @VideoId, @AwemeId, @AuthorId,
                 @AuthorNickname, @AuthorUniqueId, @Title, @Description,
                 @ViewCount, @LikeCount, @ShareCount, @CommentCount, @DownloadCount,
                 @Duration, @CreateTime, @VideoUrl, @CoverUrl, @PlayUrl,
                 @ChannelId, @ChannelTitle, @PublishedAt, @ThumbnailUrl, @Tags)
            ON CONFLICT (video_id) DO NOTHING";

        int inserted = 0, skipped = 0;
        foreach (var item in items)
        {
            int? duration = int.TryParse(item.Duration, out var d) ? d : null;
            long? createTime = long.TryParse(item.CreateTime, out var ct) ? ct : null;

            var affected = await db.ExecuteAsync(sql, new
            {
                item.ProjectId, item.KeywordSearch, item.VideoId, item.AwemeId, item.AuthorId,
                item.AuthorNickname, item.AuthorUniqueId, item.Title, item.Description,
                ViewCount = item.ViewCount ?? 0, LikeCount = item.LikeCount ?? 0,
                ShareCount = item.ShareCount ?? 0, CommentCount = item.CommentCount ?? 0,
                DownloadCount = item.DownloadCount ?? 0,
                Duration = duration, CreateTime = createTime,
                item.VideoUrl, item.CoverUrl, item.PlayUrl,
                item.ChannelId, item.ChannelTitle, item.PublishedAt, item.ThumbnailUrl, item.Tags
            });
            if (affected > 0) inserted++;
            else skipped++;
        }

        return new BulkCreateVideoResultDto { Inserted = inserted, Skipped = skipped };
    }

    public async Task<BulkCreateVideoResultDto> BulkCreateYoutubeAsync(List<CreateYoutubeVideoDto> items)
    {
        if (items == null || items.Count == 0)
            return new BulkCreateVideoResultDto { Inserted = 0, Skipped = 0 };

        using var db = _dbFactory.CreateConnection();

        var sql = @"
            INSERT INTO youtube_videos
                (project_id, keyword_search, video_id, aweme_id, author_id,
                 author_nickname, author_unique_id, title, description,
                 share_count, comment_count, download_count,
                 duration, create_time, video_url, cover_url, play_url,
                 channel_id, channel_title, published_at,
                 view_count, like_count, thumbnail_url, tags)
            VALUES
                (@ProjectId, @KeywordSearch, @VideoId, @AwemeId, @AuthorId,
                 @AuthorNickname, @AuthorUniqueId, @Title, @Description,
                 @ShareCount, @CommentCount, @DownloadCount,
                 @Duration, @CreateTime, @VideoUrl, @CoverUrl, @PlayUrl,
                 @ChannelId, @ChannelTitle, @PublishedAt,
                 @ViewCount, @LikeCount, @ThumbnailUrl, @Tags)
            ON CONFLICT (video_id) DO NOTHING";

        int inserted = 0, skipped = 0;
        foreach (var item in items)
        {
            int? duration = int.TryParse(item.Duration, out var d) ? d : null;
            long? createTime = long.TryParse(item.CreateTime, out var ct) ? ct : null;

            var affected = await db.ExecuteAsync(sql, new
            {
                item.ProjectId, item.KeywordSearch, item.VideoId, item.AwemeId, item.AuthorId,
                item.AuthorNickname, item.AuthorUniqueId, item.Title, item.Description,
                ShareCount = item.ShareCount ?? 0, CommentCount = item.CommentCount ?? 0,
                DownloadCount = item.DownloadCount ?? 0,
                Duration = duration, CreateTime = createTime,
                item.VideoUrl, item.CoverUrl, item.PlayUrl,
                item.ChannelId, item.ChannelTitle, item.PublishedAt,
                ViewCount = item.ViewCount ?? 0, LikeCount = item.LikeCount ?? 0,
                item.ThumbnailUrl, item.Tags
            });
            if (affected > 0) inserted++;
            else skipped++;
        }

        return new BulkCreateVideoResultDto { Inserted = inserted, Skipped = skipped };
    }

    public async Task<BulkCreateVideoResultDto> BulkCreateFacebookAsync(List<CreateFacebookPostDto> items)
    {
        if (items == null || items.Count == 0)
            return new BulkCreateVideoResultDto { Inserted = 0, Skipped = 0 };

        using var db = _dbFactory.CreateConnection();

        var sql = @"
            INSERT INTO facebook_posts
                (project_id, keyword_search, post_id, post_url, post_type,
                 author_id, author_name, author_url, author_avatar, message,
                 reaction_count, like_count, love_count, haha_count, wow_count, sad_count, angry_count,
                 comment_count, share_count,
                 image_urls, video_url, thumbnail_url, shared_link,
                 published_at, raw_json)
            VALUES
                (@ProjectId, @KeywordSearch, @PostId, @PostUrl, @PostType,
                 @AuthorId, @AuthorName, @AuthorUrl, @AuthorAvatar, @Message,
                 @ReactionCount, @LikeCount, @LoveCount, @HahaCount, @WowCount, @SadCount, @AngryCount,
                 @CommentCount, @ShareCount,
                 @ImageUrls, @VideoUrl, @ThumbnailUrl, @SharedLink,
                 @PublishedAt, @RawJson::jsonb)
            ON CONFLICT (post_id) DO NOTHING";

        int inserted = 0, skipped = 0;
        foreach (var item in items)
        {
            var affected = await db.ExecuteAsync(sql, new
            {
                item.ProjectId, item.KeywordSearch, item.PostId, item.PostUrl, item.PostType,
                item.AuthorId, item.AuthorName, item.AuthorUrl, item.AuthorAvatar, item.Message,
                ReactionCount = item.ReactionCount ?? 0, LikeCount = item.LikeCount ?? 0,
                LoveCount = item.LoveCount ?? 0, HahaCount = item.HahaCount ?? 0,
                WowCount = item.WowCount ?? 0, SadCount = item.SadCount ?? 0,
                AngryCount = item.AngryCount ?? 0,
                CommentCount = item.CommentCount ?? 0, ShareCount = item.ShareCount ?? 0,
                item.ImageUrls, item.VideoUrl, item.ThumbnailUrl, item.SharedLink,
                item.PublishedAt, item.RawJson
            });
            if (affected > 0) inserted++;
            else skipped++;
        }

        return new BulkCreateVideoResultDto { Inserted = inserted, Skipped = skipped };
    }

    public async Task<List<FacebookCachedPostDto>> GetFacebookCachedPostsAsync(Guid projectId)
    {
        var cacheKey = $"fb_posts:{projectId}";
        var cached = await _redis.GetAsync<List<FacebookCachedPostDto>>(cacheKey);
        if (cached != null && cached.Count > 0)
            return cached;

        // Cache miss — load from DB
        var posts = await LoadFacebookPostsFromDbAsync(projectId);
        await _redis.SetAsync(cacheKey, posts, FbCacheTtl);
        return posts;
    }

    public async Task RefreshFacebookCacheAsync(Guid projectId)
    {
        var posts = await LoadFacebookPostsFromDbAsync(projectId);
        var cacheKey = $"fb_posts:{projectId}";
        await _redis.SetAsync(cacheKey, posts, FbCacheTtl);
    }

    private async Task<List<FacebookCachedPostDto>> LoadFacebookPostsFromDbAsync(Guid projectId)
    {
        using var db = _dbFactory.CreateConnection();
        var sql = "SELECT post_id AS PostId, post_url AS PostUrl FROM facebook_posts WHERE project_id = @ProjectId";
        return (await db.QueryAsync<FacebookCachedPostDto>(sql, new { ProjectId = projectId })).ToList();
    }
}
