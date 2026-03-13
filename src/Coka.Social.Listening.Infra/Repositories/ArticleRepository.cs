using Dapper;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Infra.Data;

namespace Coka.Social.Listening.Infra.Repositories;

public class ArticleRepository : BaseRepository<ArticleEntity>, IArticleRepository
{
    public ArticleRepository(DbConnectionFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<PagedResult<ArticleDto>> GetArticlesAsync(ArticleFilterDto filter)
    {
        using var db = _dbFactory.CreateConnection();
        var articleTable = Core.Helpers.TableHelper.GetTableName<ArticleEntity>();
        var projectTable = Core.Helpers.TableHelper.GetTableName<ProjectEntity>();

        var whereClauses = new List<string> { "a.status = 1" };
        var parameters = new DynamicParameters();

        if (filter.ProjectId.HasValue)
        {
            whereClauses.Add("a.project_id = @ProjectId");
            parameters.Add("ProjectId", filter.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            whereClauses.Add("(a.title ILIKE @Search OR a.source_name ILIKE @Search)");
            parameters.Add("Search", $"%{filter.Search}%");
        }

        if (filter.FromDate.HasValue)
        {
            whereClauses.Add("a.pub_date >= @FromDate");
            parameters.Add("FromDate", filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            whereClauses.Add("a.pub_date <= @ToDate");
            parameters.Add("ToDate", filter.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Sentiment))
        {
            whereClauses.Add("a.sentiment = @Sentiment");
            parameters.Add("Sentiment", filter.Sentiment);
        }

        if (filter.IsRelevant.HasValue)
        {
            whereClauses.Add("a.is_relevant = @IsRelevant");
            parameters.Add("IsRelevant", filter.IsRelevant.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.UrlContains))
        {
            whereClauses.Add("a.url ILIKE @UrlContains");
            parameters.Add("UrlContains", $"%{filter.UrlContains}%");
        }

        var whereStr = string.Join(" AND ", whereClauses);

        // Count
        var countSql = $@"
            SELECT COUNT(*)
            FROM {articleTable} a
            LEFT JOIN {projectTable} p ON p.id = a.project_id
            WHERE {whereStr}";

        var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);

        // Data
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

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
                a.created_at    AS CreatedAt
            FROM {articleTable} a
            LEFT JOIN {projectTable} p ON p.id = a.project_id
            WHERE {whereStr}
            ORDER BY a.pub_date DESC NULLS LAST
            OFFSET @Offset LIMIT @PageSize";

        var items = (await db.QueryAsync<ArticleDto>(dataSql, parameters)).ToList();

        return new PagedResult<ArticleDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<BulkCreateArticleResultDto> BulkCreateAsync(List<CreateArticleDto> items)
    {
        if (items == null || items.Count == 0)
            return new BulkCreateArticleResultDto { Inserted = 0, Skipped = 0 };

        using var db = _dbFactory.CreateConnection();
        var articleTable = Core.Helpers.TableHelper.GetTableName<ArticleEntity>();

        var inserted = 0;
        var skipped = 0;

        // Use ON CONFLICT to skip duplicates by URL
        var sql = $@"
            INSERT INTO {articleTable}
                (id, project_id, title, url, source_name, source_url, pub_date,
                 snippet, image_url, sentiment, is_relevant, status, created_at, updated_at)
            VALUES
                (@Id, @ProjectId, @Title, @Url, @SourceName, @SourceUrl, @PubDate,
                 @Snippet, @ImageUrl, @Sentiment, @IsRelevant, 1, NOW(), NOW())
            ON CONFLICT (url) DO NOTHING";

        foreach (var item in items)
        {
            var affected = await db.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                item.ProjectId,
                item.Title,
                item.Url,
                item.SourceName,
                item.SourceUrl,
                item.PubDate,
                item.Snippet,
                item.ImageUrl,
                item.Sentiment,
                item.IsRelevant
            });

            if (affected > 0) inserted++;
            else skipped++;
        }

        return new BulkCreateArticleResultDto { Inserted = inserted, Skipped = skipped };
    }

    public async Task<bool> ExistsByUrlAsync(string url)
    {
        using var db = _dbFactory.CreateConnection();
        var articleTable = Core.Helpers.TableHelper.GetTableName<ArticleEntity>();

        var sql = $"SELECT EXISTS(SELECT 1 FROM {articleTable} WHERE url = @Url)";
        return await db.ExecuteScalarAsync<bool>(sql, new { Url = url });
    }

    public async Task<ArticleDto?> GetArticleByIdAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        var articleTable = Core.Helpers.TableHelper.GetTableName<ArticleEntity>();
        var projectTable = Core.Helpers.TableHelper.GetTableName<ProjectEntity>();

        var sql = $@"
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
                a.created_at    AS CreatedAt
            FROM {articleTable} a
            LEFT JOIN {projectTable} p ON p.id = a.project_id
            WHERE a.id = @Id AND a.status = 1";

        return await db.QueryFirstOrDefaultAsync<ArticleDto>(sql, new { Id = id });
    }

    public async Task<bool> UpdateArticleAsync(Guid id, UpdateArticleDto dto)
    {
        using var db = _dbFactory.CreateConnection();
        var articleTable = Core.Helpers.TableHelper.GetTableName<ArticleEntity>();

        var setClauses = new List<string> { "updated_at = NOW()" };
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.ProjectId.HasValue) { setClauses.Add("project_id = @ProjectId"); parameters.Add("ProjectId", dto.ProjectId.Value); }
        if (dto.Title != null)      { setClauses.Add("title = @Title");           parameters.Add("Title", dto.Title); }
        if (dto.SourceName != null)  { setClauses.Add("source_name = @SourceName"); parameters.Add("SourceName", dto.SourceName); }
        if (dto.SourceUrl != null)   { setClauses.Add("source_url = @SourceUrl"); parameters.Add("SourceUrl", dto.SourceUrl); }
        if (dto.PubDate.HasValue)    { setClauses.Add("pub_date = @PubDate");     parameters.Add("PubDate", dto.PubDate.Value); }
        if (dto.Snippet != null)     { setClauses.Add("snippet = @Snippet");       parameters.Add("Snippet", dto.Snippet); }
        if (dto.ImageUrl != null)    { setClauses.Add("image_url = @ImageUrl");   parameters.Add("ImageUrl", dto.ImageUrl); }
        if (dto.Sentiment != null)   { setClauses.Add("sentiment = @Sentiment");   parameters.Add("Sentiment", dto.Sentiment); }
        if (dto.IsRelevant.HasValue) { setClauses.Add("is_relevant = @IsRelevant"); parameters.Add("IsRelevant", dto.IsRelevant.Value); }

        var sql = $"UPDATE {articleTable} SET {string.Join(", ", setClauses)} WHERE id = @Id AND status = 1";
        var affected = await db.ExecuteAsync(sql, parameters);
        return affected > 0;
    }

    public async Task<bool> DeleteArticleAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        var articleTable = Core.Helpers.TableHelper.GetTableName<ArticleEntity>();

        var sql = $"UPDATE {articleTable} SET status = 0, updated_at = NOW() WHERE id = @Id AND status = 1";
        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<int> BulkUpdateUrlAsync(List<BulkUpdateArticleUrlItemDto> items)
    {
        if (items == null || items.Count == 0) return 0;

        using var db = _dbFactory.CreateConnection();
        var articleTable = Core.Helpers.TableHelper.GetTableName<ArticleEntity>();

        var sql = $"UPDATE {articleTable} SET url = @Url, updated_at = NOW() WHERE id = @Id AND status = 1";

        int total = 0;
        foreach (var item in items)
        {
            total += await db.ExecuteAsync(sql, new { Id = item.Id, Url = item.Url });
        }

        return total;
    }
}
