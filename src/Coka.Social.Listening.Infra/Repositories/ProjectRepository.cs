using System.Text;
using Dapper;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces;
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

    public async Task<bool> UpdateConfirmationDateAsync(Guid id, DateOnly confirmationDate)
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
                p.created_at        AS CreatedAt
            FROM {projectTable} p
            LEFT JOIN {categoryTable} c ON c.key = p.category_key
            WHERE p.status = 1 AND p.confirmation_date IS NOT NULL
            ORDER BY p.confirmation_date DESC
            LIMIT @Top";

        return (await db.QueryAsync<ProjectDto>(sql, new { Top = top })).ToList();
    }
}
