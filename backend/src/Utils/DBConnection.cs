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
    var configuredConnection = configuration.GetConnectionString("Default");
    if (!string.IsNullOrWhiteSpace(configuredConnection))
    {
      return configuredConnection;
    }

    var host = RequiredEnv("POSTGRES_HOST");
    var port = RequiredEnv("POSTGRES_PORT");
    var database = RequiredEnv("POSTGRES_DB");
    var username = RequiredEnv("POSTGRES_USER");
    var password = RequiredEnv("POSTGRES_PASSWORD");

    return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
  }

  private static string RequiredEnv(string name)
  {
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new InvalidOperationException($"{name} must be set.");
    }

    return value;
  }
}
