using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Utils;

public interface IDbConnectionFactory
{
  IDbConnection Create();
  Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken);
}

public sealed class DbConnectionFactory : IDbConnectionFactory
{
  private readonly string _connectionString;

  public DbConnectionFactory(IConfiguration configuration)
  {
    _connectionString = ResolveConnectionString(configuration);
  }

  public IDbConnection Create()
  {
    return new NpgsqlConnection(_connectionString);
  }

  public async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken)
  {
    var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);
    return connection;
  }

  private static string ResolveConnectionString(IConfiguration configuration)
  {
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
      return databaseUrl;
    }

    var configuredConnection = configuration.GetConnectionString("Default");
    if (!string.IsNullOrWhiteSpace(configuredConnection))
    {
      return configuredConnection;
    }

    var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "db";
    var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
    var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "friday";
    var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "friday";
    var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

    if (string.IsNullOrWhiteSpace(password))
    {
      throw new InvalidOperationException(
        "DATABASE_URL, ConnectionStrings:Default, or POSTGRES_PASSWORD must be set.");
    }

    return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
  }
}
