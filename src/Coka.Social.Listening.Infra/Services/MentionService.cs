using Dapper;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Services;
using Coka.Social.Listening.Infra.Data;

namespace Coka.Social.Listening.Infra.Services;

public class MentionService : IMentionService
{
    private readonly DbConnectionFactory _dbFactory;

    public MentionService(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
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

        return result;
    }
}
