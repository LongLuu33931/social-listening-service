using Dapper;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Infra.Data;

namespace Coka.Social.Listening.Infra.Repositories;

public class CategoryRepository : BaseRepository<CategoryEntity>, ICategoryRepository
{
    public CategoryRepository(DbConnectionFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<List<CategoryGroupDto>> GetCategoryTreeAsync()
    {
        using var db = _dbFactory.CreateConnection();
        var categoryTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.CategoryEntity>();
        var projectTable = Coka.Social.Listening.Core.Helpers.TableHelper.GetTableName<Coka.Social.Listening.Core.Entities.ProjectEntity>();

        // Get all categories with their project counts in a single query
        var sql = $@"
            SELECT
                c.id            AS Id,
                c.key           AS Key,
                c.label         AS Label,
                c.group_key     AS GroupKey,
                c.group_label   AS GroupLabel,
                COALESCE(pc.project_count, 0) AS ProjectCount
            FROM {categoryTable} c
            LEFT JOIN (
                SELECT category_key, COUNT(*) AS project_count
                FROM {projectTable}
                GROUP BY category_key
            ) pc ON pc.category_key = c.key
            WHERE c.status = 1
            ORDER BY c.group_key, c.label";

        var rows = (await db.QueryAsync<CategoryChildRow>(sql)).ToList();

        // Group into parent → children tree
        var groups = rows
            .GroupBy(r => new { r.GroupKey, r.GroupLabel })
            .Select(g => new CategoryGroupDto
            {
                GroupKey = g.Key.GroupKey ?? "",
                GroupLabel = g.Key.GroupLabel ?? "",
                TotalProjectCount = g.Sum(x => x.ProjectCount),
                Children = g.Select(x => new CategoryChildDto
                {
                    Id = x.Id,
                    Key = x.Key,
                    Label = x.Label,
                    ProjectCount = x.ProjectCount
                }).ToList()
            })
            .ToList();

        return groups;
    }

    // Internal row model for Dapper mapping
    private class CategoryChildRow
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? GroupKey { get; set; }
        public string? GroupLabel { get; set; }
        public int ProjectCount { get; set; }
    }
}
