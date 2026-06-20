using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
  public async Task<Result<IReadOnlyCollection<DiscordServer>, AppError>> GetDiscordServers(
    IDbConnection connection,
    DashboardQuery query,
    CancellationToken cancellationToken)
  {
    var parameters = new DynamicParameters();
    var where = string.Empty;

    if (!string.IsNullOrWhiteSpace(query.Search))
    {
      where = "WHERE name ILIKE @Search OR server_code ILIKE @Search";
      parameters.Add("Search", $"%{query.Search.Trim()}%");
    }

    var command = new CommandDefinition(
      $"""
      SELECT
        server_id,
        name,
        server_code,
        created_at
      FROM discord.servers
      {where}
      ORDER BY name;
      """,
      parameters,
      cancellationToken: cancellationToken);

    try
    {
      var servers = await connection.QueryAsync<DiscordServer>(command);
      return Result<IReadOnlyCollection<DiscordServer>, AppError>.Ok(servers.ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<DiscordServer>, AppError>.Fail(
        AppError.ServerError($"Failed to get Discord servers: {ex.Message}"));
    }
  }
}
