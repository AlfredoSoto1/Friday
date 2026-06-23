using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
  public async Task<Result<IReadOnlyCollection<DiscordServer>, AppError>> GetDiscordServers(IDbConnection connection, DashboardQuery query)
  {
    var parameters = new DynamicParameters();
    parameters.Add("Limit", query.Limit);
    parameters.Add("Offset", query.PageIndex * query.Limit);

    try
    {
      const string sql = @"
        SELECT server_id, name, guild_id, created_at
          FROM discord.servers
        ORDER BY created_at DESC
        LIMIT @Limit OFFSET @Offset;
      ";

      var servers = await connection.QueryAsync(sql, parameters);
      return Result<IReadOnlyCollection<DiscordServer>, AppError>.Ok(servers.Select(MapToDiscordServer).ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<DiscordServer>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }


  public async Task<Result<BackendStatus, AppError>> GetStatus(IDbConnection connection)
  {
    try
    {
      await connection.ExecuteScalarAsync<int>("SELECT 1;");
      return Result<BackendStatus, AppError>.Ok(new BackendStatus
      {
        Status = "ok",
        Database = "connected",
        CheckedAt = DateTimeOffset.UtcNow
      });
    }
    catch (Exception ex)
    {
      return Result<BackendStatus, AppError>.Ok(new BackendStatus
      {
        Status = "degraded",
        Database = $"disconnected: {ex.Message}",
        CheckedAt = DateTimeOffset.UtcNow
      });
    }
  }

  public async Task<Result<CatalogSummary, AppError>> GetCatalogSummary(IDbConnection connection)
  {
    try
    {
      const string tablesSql = @"
        SELECT table_schema, table_name
          FROM information_schema.tables
        WHERE table_schema IN ('discord', 'inelicom')
          AND table_type = 'BASE TABLE'
        ORDER BY table_schema, table_name;
      ";

      var tables = (await connection.QueryAsync(tablesSql)).ToArray();
      var summaries = new List<TableSummary>();

      foreach (var table in tables)
      {
        var schema = (string)table.table_schema;
        var name = (string)table.table_name;
        var rows = await connection.ExecuteScalarAsync<long>(
          $"SELECT COUNT(*) FROM {QuoteIdentifier(schema)}.{QuoteIdentifier(name)};");

        summaries.Add(new TableSummary
        {
          Schema = schema,
          Table = name,
          Rows = rows
        });
      }

      return Result<CatalogSummary, AppError>.Ok(new CatalogSummary
      {
        Schemas = summaries
          .GroupBy(table => table.Schema)
          .Select(group => new SchemaSummary
          {
            Name = group.Key,
            Tables = group.ToArray()
          })
          .ToArray()
      });
    }
    catch (Exception ex)
    {
      return Result<CatalogSummary, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static DiscordServer MapToDiscordServer(dynamic record) => new()
  {
    ServerId = (int)record.server_id,
    Name = (string)record.name,
    ServerCode = (string)record.guild_id,
    CreatedAt = (DateTime)record.created_at
  };

  private static string QuoteIdentifier(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
