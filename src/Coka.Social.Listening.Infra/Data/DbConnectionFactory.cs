using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using Coka.Social.Listening.Infra.Helpers;

namespace Coka.Social.Listening.Infra.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        var encrypted = configuration["Database:EncryptedConnectionString"]
            ?? throw new InvalidOperationException("Database:EncryptedConnectionString is not configured.");

        _connectionString = EncryptionHelper.Decrypt(encrypted);
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
