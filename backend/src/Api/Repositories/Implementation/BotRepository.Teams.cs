using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class BotRepository
{
  public async Task<Result<IReadOnlyCollection<GuildTeam>, AppError>> GetGuildTeams(
    IDbConnection connection, long guildId)
  {
    try
    {
      const string sql = @"
        SELECT teams.team_id, teams.position, teams.name,
               COUNT(DISTINCT user_teams.su_id) AS member_count
          FROM discord.teams
          JOIN discord.servers USING (server_id)
          LEFT JOIN discord.user_teams USING (team_id)
         WHERE servers.guild_id = @GuildId
         GROUP BY teams.team_id
         ORDER BY teams.position;
      ";
      var teams = await connection.QueryAsync<GuildTeam>(
        sql, new { GuildId = guildId.ToString() });
      return Result<IReadOnlyCollection<GuildTeam>, AppError>.Ok(teams.ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<GuildTeam>, AppError>.Fail(
        AppError.BadRequest(ex.Message));
    }
  }
}
