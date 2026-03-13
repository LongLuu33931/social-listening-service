using Dapper;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Helpers;
using Coka.Social.Listening.Core.Interfaces.Services;
using Coka.Social.Listening.Infra.Data;
using Coka.Social.Listening.Infra.Helpers;

namespace Coka.Social.Listening.Infra.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly RedisHelper _redis;
    private readonly DbConnectionFactory _dbFactory;
    private const string CacheKey = "projects";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public RedisCacheService(RedisHelper redis, DbConnectionFactory dbFactory)
    {
        _redis = redis;
        _dbFactory = dbFactory;
    }

    public async Task<List<string>> GetProjectNamesAsync()
    {
        var cached = await _redis.GetAsync<List<string>>(CacheKey);
        if (cached != null && cached.Count > 0)
            return cached;

        // Cache miss — load from DB, cache, and return
        var names = await LoadProjectNamesFromDbAsync();
        await _redis.SetAsync(CacheKey, names, CacheTtl);
        return names;
    }

    public async Task RefreshProjectNamesCacheAsync()
    {
        var names = await LoadProjectNamesFromDbAsync();
        await _redis.SetAsync(CacheKey, names, CacheTtl);
    }

    private async Task<List<string>> LoadProjectNamesFromDbAsync()
    {
        using var db = _dbFactory.CreateConnection();
        var tableName = TableHelper.GetTableName<ProjectEntity>();
        var sql = $"SELECT name FROM {tableName} WHERE status = 1 ORDER BY name";
        return (await db.QueryAsync<string>(sql)).ToList();
    }
}
