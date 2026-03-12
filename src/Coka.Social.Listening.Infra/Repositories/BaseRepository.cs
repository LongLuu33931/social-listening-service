using System.Reflection;
using Dapper;
using Coka.Social.Listening.Core.Attributes;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Helpers;
using Coka.Social.Listening.Core.Interfaces;
using Coka.Social.Listening.Infra.Data;

namespace Coka.Social.Listening.Infra.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DbConnectionFactory _dbFactory;
    protected readonly string _tableName;
    protected readonly List<PropertyInfo> _properties;
    protected readonly string _selectAllColumns;

    protected BaseRepository(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
        _tableName = TableHelper.GetTableName<TEntity>();
        
        _properties = typeof(TEntity).GetProperties()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .ToList();
            
        var selectCols = _properties.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()!.Name} AS {p.Name}");
        _selectAllColumns = string.Join(", ", selectCols);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        var sql = $"SELECT {_selectAllColumns} FROM {_tableName} WHERE id = @Id";
        return await db.QuerySingleOrDefaultAsync<TEntity>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        using var db = _dbFactory.CreateConnection();
        var sql = $"SELECT {_selectAllColumns} FROM {_tableName} ORDER BY created_at DESC";
        return await db.QueryAsync<TEntity>(sql);
    }

    public virtual async Task<Guid> AddAsync(TEntity entity)
    {
        using var db = _dbFactory.CreateConnection();
        
        if (entity.Id == Guid.Empty)
            entity.Id = Guid.NewGuid();
            
        entity.CreatedAt = DateTime.Now;
        entity.UpdatedAt = DateTime.Now;

        var props = _properties.Where(p => p.Name != "Id"); // ID is handled separately
        var cols = string.Join(", ", props.Select(p => p.GetCustomAttribute<ColumnAttribute>()!.Name));
        var vars = string.Join(", ", props.Select(p => "@" + p.Name));

        var sql = $"INSERT INTO {_tableName} (id, {cols}) VALUES (@Id, {vars}) RETURNING id";
        return await db.ExecuteScalarAsync<Guid>(sql, entity);
    }

    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        using var db = _dbFactory.CreateConnection();
        
        entity.UpdatedAt = DateTime.Now;

        var props = _properties.Where(p => p.Name != "Id" && p.Name != "CreatedAt");
        var sets = string.Join(", ", props.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()!.Name} = @{p.Name}"));

        var sql = $"UPDATE {_tableName} SET {sets} WHERE id = @Id";
        var affected = await db.ExecuteAsync(sql, entity);
        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        var sql = $"DELETE FROM {_tableName} WHERE id = @Id";
        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}
