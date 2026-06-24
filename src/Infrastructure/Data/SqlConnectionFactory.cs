using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Fashia.Infrastructure.Data;

public interface ISqlConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("FashiaDb")
            ?? throw new InvalidOperationException("Connection string 'FashiaDb' was not found.");
    }

    public async Task<IDbConnection> CreateConnectionAsync(
        CancellationToken cancellationToken = default
    )
    {
        var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}
