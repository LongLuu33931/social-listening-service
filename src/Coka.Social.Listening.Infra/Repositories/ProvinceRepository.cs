using Dapper;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Infra.Data;

namespace Coka.Social.Listening.Infra.Repositories;

public class ProvinceRepository : BaseRepository<ProvinceEntity>, IProvinceRepository
{
    public ProvinceRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

    private static string SelectColumns => @"
        id                  AS Id,
        name                AS Name,
        slug                AS Slug,
        center              AS Center,
        region              AS Region,
        merged_from         AS MergedFrom,
        natural_area_km2    AS NaturalAreaKm2,
        population_thousands AS PopulationThousands,
        coords              AS Coords";

    public async Task<List<ProvinceDto>> GetAllAsync(ProvinceFilterDto filter)
    {
        using var db = _dbFactory.CreateConnection();
        var table = Core.Helpers.TableHelper.GetTableName<ProvinceEntity>();

        var whereClauses = new List<string> { "status = 1" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            whereClauses.Add("(name ILIKE @Search OR center ILIKE @Search)");
            parameters.Add("Search", $"%{filter.Search}%");
        }

        if (!string.IsNullOrWhiteSpace(filter.Region))
        {
            whereClauses.Add("region = @Region");
            parameters.Add("Region", filter.Region.ToLower());
        }

        var where = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT {SelectColumns}
            FROM {table}
            WHERE {where}
            ORDER BY region, name";

        return (await db.QueryAsync<ProvinceDto>(sql, parameters)).ToList();
    }

    public new async Task<ProvinceDto?> GetByIdAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        var table = Core.Helpers.TableHelper.GetTableName<ProvinceEntity>();

        var sql = $"SELECT {SelectColumns} FROM {table} WHERE id = @Id AND status = 1";
        return await db.QueryFirstOrDefaultAsync<ProvinceDto>(sql, new { Id = id });
    }

    public async Task<ProvinceDto?> GetBySlugAsync(string slug)
    {
        using var db = _dbFactory.CreateConnection();
        var table = Core.Helpers.TableHelper.GetTableName<ProvinceEntity>();

        var sql = $"SELECT {SelectColumns} FROM {table} WHERE slug = @Slug AND status = 1";
        return await db.QueryFirstOrDefaultAsync<ProvinceDto>(sql, new { Slug = slug });
    }
}
