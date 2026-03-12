using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Coka.Social.Listening.Infra.Data;

public class RedisConnectionFactory
{
    private readonly Lazy<ConnectionMultiplexer> _connection;
    private readonly int _database;

    public RedisConnectionFactory(IConfiguration configuration)
    {
        var host = configuration["Redis:Host"] ?? "localhost";
        var port = configuration["Redis:Port"] ?? "6379";
        var password = configuration["Redis:Password"] ?? "";
        _database = int.Parse(configuration["Redis:Database"] ?? "4");

        var options = ConfigurationOptions.Parse($"{host}:{port}");
        if (!string.IsNullOrEmpty(password))
            options.Password = password;
        options.AbortOnConnectFail = false;

        _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
    }

    public IDatabase GetDatabase() => _connection.Value.GetDatabase(_database);
}
