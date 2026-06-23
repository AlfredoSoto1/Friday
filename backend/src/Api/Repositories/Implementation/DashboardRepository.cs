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

  private static DiscordServer MapToDiscordServer(dynamic record) => new()
  {
    ServerId = (int)record.server_id,
    Name = (string)record.name,
    ServerCode = (string)record.guild_id,
    CreatedAt = (DateTime)record.created_at
  };
}
