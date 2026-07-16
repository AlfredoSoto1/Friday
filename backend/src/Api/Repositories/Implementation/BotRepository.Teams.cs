using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class BotRepository
{
  public async Task<Result<RosterMembersContextResult, AppError>> ClearAutomaticRosterRoles(
    IDbConnection connection, IDbTransaction transaction, long guildId,
    IReadOnlyCollection<RosterUserReference> users,
    IReadOnlyCollection<RosterMemberReference> members)
  {
    try
    {
      const string sql = @"
        DELETE FROM discord.user_roles
         USING discord.roles
          JOIN discord.servers USING (server_id)
         WHERE user_roles.role_id = roles.role_id
           AND user_roles.su_id = ANY(@ServerUserIds)
           AND servers.guild_id = @GuildId
           AND UPPER(roles.name) = ANY(@RoleNames);
      ";
      await connection.ExecuteAsync(sql, new
      {
        GuildId = guildId.ToString(),
        RoleNames = new[] { "INEL", "ICOM", "INSO", "CIIC", "PREPA" },
        ServerUserIds = members.Select(member => member.ServerUserId).ToArray()
      }, transaction);

      return Result<RosterMembersContextResult, AppError>.Ok(new()
      {
        Users = users,
        Members = members
      });
    }
    catch (Exception ex)
    {
      return Result<RosterMembersContextResult, AppError>.Fail(
        AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<IReadOnlyCollection<GuildTeam>, AppError>> GetGuildTeams(
    IDbConnection connection, long guildId)
  {
    try
    {
      const string sql = @"
        SELECT teams.team_id, teams.position, teams.name, teams.role_id,
               primary_roles.name AS role_name,
               CASE
                 WHEN teams.role_id IS NULL THEN
                   COALESCE(
                     ARRAY_AGG(DISTINCT team_roles.role_id ORDER BY team_roles.role_id)
                       FILTER (WHERE team_roles.role_id IS NOT NULL),
                     ARRAY[]::INT[])
                 ELSE ARRAY[teams.role_id] || COALESCE(
                   ARRAY_AGG(DISTINCT team_roles.role_id ORDER BY team_roles.role_id)
                     FILTER (WHERE team_roles.role_id <> teams.role_id),
                   ARRAY[]::INT[])
               END AS role_ids,
               COUNT(DISTINCT user_teams.su_id) AS member_count
          FROM discord.teams
          JOIN discord.servers USING (server_id)
          LEFT JOIN discord.roles AS primary_roles
            ON primary_roles.role_id = teams.role_id
          LEFT JOIN discord.team_roles USING (team_id)
          LEFT JOIN discord.user_teams USING (team_id)
         WHERE servers.guild_id = @GuildId
         GROUP BY teams.team_id, primary_roles.name
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
