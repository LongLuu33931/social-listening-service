using System.Text;
using Dapper;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Infra.Data;

namespace Coka.Social.Listening.Infra.Repositories;

public class ProjectRepository : BaseRepository<ProjectEntity>, IProjectRepository
{
    public ProjectRepository(DbConnectionFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();
        var categoryTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.CategoryEntity>();

        var whereClauses = new List<string> { "p.status = 1" };
        var parameters = new DynamicParameters();

        // Filter by category_key
        if (!string.IsNullOrWhiteSpace(filter.CategoryKey))
        {
            whereClauses.Add("p.category_key = @CategoryKey");
            parameters.Add("CategoryKey", filter.CategoryKey);
        }

        // Filter by group_key (all categories in that group)
        if (!string.IsNullOrWhiteSpace(filter.GroupKey))
        {
            whereClauses.Add("c.group_key = @GroupKey");
            parameters.Add("GroupKey", filter.GroupKey);
        }

        // Search by project name or investor
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            whereClauses.Add("(p.name ILIKE @Search OR p.investor ILIKE @Search)");
            parameters.Add("Search", $"%{filter.Search}%");
        }

        // Filter by missing investor
        if (filter.MissingInvestor == true)
        {
            whereClauses.Add("(p.investor IS NULL OR Trim(p.investor) = '' OR p.investor ILIKE 'Đang cập nhật')");
        }

        // Filter by missing confirmation date
        if (filter.MissingConfirmationDate == true)
        {
            whereClauses.Add("p.confirmation_date IS NULL");
        }

        var whereStr = string.Join(" AND ", whereClauses);

        // Count query
        var countSql = $@"
            SELECT COUNT(*)
            FROM {projectTable} p
            LEFT JOIN {categoryTable} c ON c.key = p.category_key
            WHERE {whereStr}";

        var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);

        // Data query with pagination
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var dataSql = $@"
            SELECT
                p.id                AS Id,
                p.name              AS Name,
                p.source            AS Source,
                p.location          AS Location,
                p.investor          AS Investor,
                p.origin_url        AS OriginUrl,
                p.logo              AS Logo,
                p.category_key      AS CategoryKey,
                c.label             AS CategoryLabel,
                c.group_key         AS GroupKey,
                c.group_label       AS GroupLabel,
                p.status            AS Status,
                p.confirmation_date AS ConfirmationDate,
                p.province_id       AS ProvinceId,
                (
                    (SELECT COUNT(*) FROM articles art WHERE art.project_id = p.id AND art.status = 1 AND art.is_approved = true) +
                    (SELECT COUNT(*) FROM tiktok_videos tv WHERE tv.project_id = p.id AND tv.is_approved = true) +
                    (SELECT COUNT(*) FROM youtube_videos yv WHERE yv.project_id = p.id AND yv.is_approved = true)
                ) AS TotalMentions,
                p.created_at        AS CreatedAt
            FROM {projectTable} p
            LEFT JOIN {categoryTable} c ON c.key = p.category_key
            WHERE {whereStr}
            ORDER BY p.created_at DESC
            OFFSET @Offset LIMIT @PageSize";

        var items = (await db.QueryAsync<ProjectDto>(dataSql, parameters)).ToList();

        return new PagedResult<ProjectDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<bool> UpdateInvestorAsync(Guid id, string investor)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();

        var sql = $@"
            UPDATE {projectTable}
            SET investor = @Investor, updated_at = NOW()
            WHERE id = @Id";

        var affected = await db.ExecuteAsync(sql, new { Id = id, Investor = investor });
        return affected > 0;
    }

    public async Task<bool> UpdateConfirmationDateAsync(Guid id, DateTime confirmationDate)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();

        var sql = $@"
            UPDATE {projectTable}
            SET confirmation_date = @ConfirmationDate, updated_at = NOW()
            WHERE id = @Id";

        var affected = await db.ExecuteAsync(sql, new { Id = id, ConfirmationDate = confirmationDate });
        return affected > 0;
    }

    public async Task<bool> HideAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();
        
        var sql = $@"
            UPDATE {projectTable}
            SET status = 2, updated_at = NOW()
            WHERE id = @Id";

        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> UpdateProjectAsync(Guid id, UpdateProjectDto dto)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();

        var setClauses = new List<string> { "updated_at = NOW()" };
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.Name != null)              { setClauses.Add("name = @Name");                       parameters.Add("Name", dto.Name); }
        if (dto.Source != null)            { setClauses.Add("source = @Source");                     parameters.Add("Source", dto.Source); }
        if (dto.Location != null)          { setClauses.Add("location = @Location");                 parameters.Add("Location", dto.Location); }
        if (dto.Investor != null)          { setClauses.Add("investor = @Investor");                 parameters.Add("Investor", dto.Investor); }
        if (dto.OriginUrl != null)         { setClauses.Add("origin_url = @OriginUrl");             parameters.Add("OriginUrl", dto.OriginUrl); }
        if (dto.Logo != null)             { setClauses.Add("logo = @Logo");                         parameters.Add("Logo", dto.Logo); }
        if (dto.CategoryKey != null)       { setClauses.Add("category_key = @CategoryKey");         parameters.Add("CategoryKey", dto.CategoryKey); }
        if (dto.ConfirmationDate.HasValue) { setClauses.Add("confirmation_date = @ConfirmationDate"); parameters.Add("ConfirmationDate", dto.ConfirmationDate.Value); }
        if (dto.ProvinceId.HasValue)       { setClauses.Add("province_id = @ProvinceId");           parameters.Add("ProvinceId", dto.ProvinceId.Value); }

        var sql = $"UPDATE {projectTable} SET {string.Join(", ", setClauses)} WHERE id = @Id AND status = 1";
        var affected = await db.ExecuteAsync(sql, parameters);
        return affected > 0;
    }

    public async Task<int> BulkUpdateInvestorAsync(List<BulkUpdateInvestorItemDto> items)
    {
        if (items == null || items.Count == 0) return 0;

        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();

        var sql = $@"
            UPDATE {projectTable}
            SET investor = @Investor, updated_at = NOW()
            WHERE id = @Id";

        var affected = await db.ExecuteAsync(sql, items.Select(i => new { Id = i.Id, Investor = i.Investor }));
        return affected;
    }

    public async Task<int> BulkUpdateConfirmationDateAsync(List<BulkUpdateConfirmationDateItemDto> items)
    {
        if (items == null || items.Count == 0) return 0;

        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();

        var sql = $@"
            UPDATE {projectTable}
            SET confirmation_date = @ConfirmationDate, updated_at = NOW()
            WHERE id = @Id";

        var affected = await db.ExecuteAsync(sql, items.Select(i => new { Id = i.Id, ConfirmationDate = i.ConfirmationDate }));
        return affected;
    }

    public async Task<List<ProjectDto>> GetTopByConfirmationDateAsync(int top = 10)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();
        var categoryTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.CategoryEntity>();

        var sql = $@"
            SELECT
                p.id                AS Id,
                p.name              AS Name,
                p.source            AS Source,
                p.location          AS Location,
                p.investor          AS Investor,
                p.origin_url        AS OriginUrl,
                p.logo              AS Logo,
                p.category_key      AS CategoryKey,
                c.label             AS CategoryLabel,
                c.group_key         AS GroupKey,
                c.group_label       AS GroupLabel,
                p.status            AS Status,
                p.confirmation_date AS ConfirmationDate,
                p.province_id       AS ProvinceId,
                (
                    (SELECT COUNT(*) FROM articles art WHERE art.project_id = p.id AND art.status = 1) +
                    (SELECT COUNT(*) FROM tiktok_videos tv WHERE tv.project_id = p.id) +
                    (SELECT COUNT(*) FROM youtube_videos yv WHERE yv.project_id = p.id)
                ) AS TotalMentions,
                p.created_at        AS CreatedAt
            FROM {projectTable} p
            LEFT JOIN {categoryTable} c ON c.key = p.category_key
            WHERE p.status = 1 AND p.confirmation_date IS NOT NULL
            ORDER BY p.confirmation_date DESC
            LIMIT @Top";

        return (await db.QueryAsync<ProjectDto>(sql, new { Top = top })).ToList();
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();
        var categoryTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.CategoryEntity>();

        var sql = $@"
            SELECT
                p.id                AS Id,
                p.name              AS Name,
                p.source            AS Source,
                p.location          AS Location,
                p.investor          AS Investor,
                p.origin_url        AS OriginUrl,
                p.logo              AS Logo,
                p.category_key      AS CategoryKey,
                c.label             AS CategoryLabel,
                c.group_key         AS GroupKey,
                c.group_label       AS GroupLabel,
                p.status            AS Status,
                p.confirmation_date AS ConfirmationDate,
                p.province_id       AS ProvinceId,
                (
                    (SELECT COUNT(*) FROM articles art WHERE art.project_id = p.id AND art.status = 1) +
                    (SELECT COUNT(*) FROM tiktok_videos tv WHERE tv.project_id = p.id) +
                    (SELECT COUNT(*) FROM youtube_videos yv WHERE yv.project_id = p.id)
                ) AS TotalMentions,
                p.created_at        AS CreatedAt
            FROM {projectTable} p
            LEFT JOIN {categoryTable} c ON c.key = p.category_key
            WHERE p.id = @Id";

        return await db.QueryFirstOrDefaultAsync<ProjectDto>(sql, new { Id = id });
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsRankedByMentionsAsync(int page, int pageSize)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();
        var categoryTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.CategoryEntity>();

        var countSql = $"SELECT COUNT(*) FROM {projectTable} WHERE status = 1";
        var totalCount = await db.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var parameters = new DynamicParameters();
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var dataSql = $@"
            SELECT
                p.id                AS Id,
                p.name              AS Name,
                p.source            AS Source,
                p.location          AS Location,
                p.investor          AS Investor,
                p.origin_url        AS OriginUrl,
                p.logo              AS Logo,
                p.category_key      AS CategoryKey,
                c.label             AS CategoryLabel,
                c.group_key         AS GroupKey,
                c.group_label       AS GroupLabel,
                p.status            AS Status,
                p.confirmation_date AS ConfirmationDate,
                p.province_id       AS ProvinceId,
                (
                    (SELECT COUNT(*) FROM articles art WHERE art.project_id = p.id AND art.status = 1 AND art.is_approved = true) +
                    (SELECT COUNT(*) FROM tiktok_videos tv WHERE tv.project_id = p.id AND tv.is_approved = true) +
                    (SELECT COUNT(*) FROM youtube_videos yv WHERE yv.project_id = p.id AND yv.is_approved = true)
                ) AS TotalMentions,
                p.created_at        AS CreatedAt
            FROM {projectTable} p
            LEFT JOIN {categoryTable} c ON c.key = p.category_key
            WHERE p.status = 1
            ORDER BY TotalMentions DESC, p.name ASC
            OFFSET @Offset LIMIT @PageSize";

        var items = (await db.QueryAsync<ProjectDto>(dataSql, parameters)).ToList();

        return new PagedResult<ProjectDto>
        {
            Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<bool> ShowAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();
        
        var sql = $@"
            UPDATE {projectTable}
            SET status = 1, updated_at = NOW()
            WHERE id = @Id AND status = 2";

        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<PagedResult<ArticleDto>> GetProjectArticlesAsync(Guid projectId, int page, int pageSize)
    {
        using var db = _dbFactory.CreateConnection();
        var articleTable = Core.Helpers.TableHelper.GetTableName<Core.Entities.ArticleEntity>();
        var projectTable = Core.Helpers.TableHelper.GetTableName<Core.Entities.ProjectEntity>();

        var parameters = new DynamicParameters();
        parameters.Add("ProjectId", projectId);

        var countSql = $"SELECT COUNT(*) FROM {articleTable} WHERE project_id = @ProjectId AND status = 1";
        var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var dataSql = $@"
            SELECT
                a.id            AS Id,
                a.project_id    AS ProjectId,
                p.name          AS ProjectName,
                a.title         AS Title,
                a.url           AS Url,
                a.source_name   AS SourceName,
                a.source_url    AS SourceUrl,
                a.pub_date      AS PubDate,
                a.snippet       AS Snippet,
                a.image_url     AS ImageUrl,
                a.sentiment     AS Sentiment,
                a.is_relevant   AS IsRelevant,
                a.status        AS Status,
                a.is_approved   AS IsApproved,
                a.created_at    AS CreatedAt
            FROM {articleTable} a
            LEFT JOIN {projectTable} p ON p.id = a.project_id
            WHERE a.project_id = @ProjectId AND a.status = 1
            ORDER BY a.pub_date DESC NULLS LAST
            OFFSET @Offset LIMIT @PageSize";

        var items = (await db.QueryAsync<ArticleDto>(dataSql, parameters)).ToList();

        return new PagedResult<ArticleDto>
        {
            Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<TiktokMentionDto>> GetProjectTiktokVideosAsync(Guid projectId, int page, int pageSize)
    {
        using var db = _dbFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("ProjectId", projectId);

        var countSql = "SELECT COUNT(*) FROM tiktok_videos WHERE project_id = @ProjectId";
        var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var dataSql = @"
            SELECT
                id              AS Id,
                video_id        AS VideoId,
                author_nickname AS AuthorNickname,
                author_unique_id AS AuthorUniqueId,
                title           AS Title,
                description     AS Description,
                duration        AS Duration,
                cover_url       AS CoverUrl,
                video_url       AS VideoUrl,
                create_time     AS CreateTime,
                is_approved     AS IsApproved
            FROM tiktok_videos
            WHERE project_id = @ProjectId
            ORDER BY create_time DESC NULLS LAST
            OFFSET @Offset LIMIT @PageSize";

        var items = (await db.QueryAsync<TiktokMentionDto>(dataSql, parameters)).ToList();

        return new PagedResult<TiktokMentionDto>
        {
            Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<YoutubeMentionDto>> GetProjectYoutubeVideosAsync(Guid projectId, int page, int pageSize)
    {
        using var db = _dbFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("ProjectId", projectId);

        var countSql = "SELECT COUNT(*) FROM youtube_videos WHERE project_id = @ProjectId";
        var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var dataSql = @"
            SELECT
                id              AS Id,
                video_id        AS VideoId,
                channel_id      AS ChannelId,
                channel_title   AS ChannelTitle,
                title           AS Title,
                description     AS Description,
                view_count      AS ViewCount,
                like_count      AS LikeCount,
                comment_count   AS CommentCount,
                thumbnail_url   AS ThumbnailUrl,
                video_url       AS VideoUrl,
                published_at    AS PublishedAt,
                tags            AS Tags,
                is_approved     AS IsApproved
            FROM youtube_videos
            WHERE project_id = @ProjectId
            ORDER BY published_at DESC NULLS LAST
            OFFSET @Offset LIMIT @PageSize";

        var items = (await db.QueryAsync<YoutubeMentionDto>(dataSql, parameters)).ToList();

        return new PagedResult<YoutubeMentionDto>
        {
            Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }
}
