using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class BotRepository
{
  public async Task<Result<IReadOnlyCollection<PrepaTeamExportRow>, AppError>>
    GetGuildPrepaTeamExport(IDbConnection connection, long guildId)
  {
    try
    {
      const string sql = @"
        WITH prepa_members AS (
          SELECT user_teams.team_id,
                 servers_users.su_id AS server_user_id,
                 users.first_name,
                 users.first_last_name,
                 users.second_last_name,
                 users.initial,
                 users.email,
                 (
                   SELECT UPPER(TRIM(program_role.name))
                     FROM discord.user_roles AS program_user_role
                     JOIN discord.roles AS program_role
                       ON program_role.role_id = program_user_role.role_id
                    WHERE program_user_role.su_id = servers_users.su_id
                      AND program_role.server_id = servers_users.server_id
                      AND UPPER(TRIM(program_role.name)) IN
                          ('INEL', 'ICOM', 'INSO', 'CIIC')
                    ORDER BY CASE UPPER(TRIM(program_role.name))
                      WHEN 'INEL' THEN 1
                      WHEN 'ICOM' THEN 2
                      WHEN 'INSO' THEN 3
                      WHEN 'CIIC' THEN 4
                    END
                    LIMIT 1
                 ) AS program
            FROM discord.user_teams
            JOIN discord.servers_users USING (su_id)
            JOIN discord.users USING (user_id)
            JOIN discord.teams AS member_team
              ON member_team.team_id = user_teams.team_id
             AND member_team.server_id = servers_users.server_id
            JOIN discord.servers AS member_server
              ON member_server.server_id = servers_users.server_id
           WHERE EXISTS (
             SELECT 1
               FROM discord.user_roles AS prepa_user_role
               JOIN discord.roles AS prepa_role
                 ON prepa_role.role_id = prepa_user_role.role_id
              WHERE prepa_user_role.su_id = servers_users.su_id
                AND prepa_role.server_id = servers_users.server_id
                AND LOWER(TRIM(prepa_role.name)) IN ('prepa', 'prepas')
           )
             AND member_server.guild_id = @GuildId
        )
        SELECT teams.team_id,
               teams.position,
               teams.name AS team_name,
               prepa_members.server_user_id,
               prepa_members.first_name,
               prepa_members.first_last_name,
               prepa_members.second_last_name,
               prepa_members.initial,
               prepa_members.email,
               prepa_members.program
          FROM discord.teams
          JOIN discord.servers USING (server_id)
          LEFT JOIN prepa_members USING (team_id)
         WHERE servers.guild_id = @GuildId
         ORDER BY teams.position,
                  prepa_members.first_last_name NULLS LAST,
                  prepa_members.second_last_name NULLS LAST,
                  prepa_members.first_name NULLS LAST;
      ";

      var rows = await connection.QueryAsync<PrepaTeamExportRow>(
        sql, new { GuildId = guildId.ToString() });

      return Result<IReadOnlyCollection<PrepaTeamExportRow>, AppError>.Ok(
        rows.ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<PrepaTeamExportRow>, AppError>.Fail(
        AppError.BadRequest(ex.Message));
    }
  }
}
