using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class BotRepository
{
  public async Task<Result<RosterMembersContextResult, AppError>> AssignProgramRoles(
    IDbConnection connection, IDbTransaction transaction, long guildId,
    IReadOnlyCollection<RosterStudentAssignment> students,
    IReadOnlyCollection<RosterUserReference> users,
    IReadOnlyCollection<RosterMemberReference> members)
  {
    try
    {
      const string sql = @"
        WITH roster_assignments AS (
          SELECT email, UPPER(program) AS program
            FROM UNNEST(@Emails, @Programs) AS roster(email, program)
          UNION ALL
          SELECT email, 'PREPA' AS program
            FROM UNNEST(@Emails) AS roster(email)
        ), program_roles AS (
          SELECT roles.role_id, UPPER(roles.name) AS program
            FROM discord.roles
            JOIN discord.servers USING (server_id)
           WHERE servers.guild_id = @GuildId
             AND UPPER(roles.name) = ANY(@RoleNames)
        ), removed AS (
          DELETE FROM discord.user_roles
           USING program_roles
           WHERE user_roles.role_id = program_roles.role_id
             AND user_roles.su_id = ANY(@ServerUserIds)
        ), assigned AS (
          INSERT INTO discord.user_roles (su_id, role_id)
          SELECT members.su_id, program_roles.role_id
            FROM roster_assignments
            JOIN discord.users ON users.email = roster_assignments.email
            JOIN discord.servers_users AS members USING (user_id)
            JOIN program_roles
              ON program_roles.program = roster_assignments.program
           WHERE members.su_id = ANY(@ServerUserIds)
          ON CONFLICT DO NOTHING
          RETURNING su_id
        )
        SELECT COUNT(*) FROM assigned;
      ";
      var count = await connection.QuerySingleAsync<int>(sql, new
      {
        GuildId = guildId.ToString(),
        Emails = students.Select(student => student.Email).ToArray(),
        Programs = students.Select(student => student.Program).ToArray(),
        RoleNames = students.Select(student => student.Program).Append("PREPA").Distinct().ToArray(),
        ServerUserIds = members.Select(member => member.ServerUserId).ToArray()
      }, transaction);

      return count == students.Count * 2
        ? Result<RosterMembersContextResult, AppError>.Ok(new()
        {
          Users = users,
          Members = members
        })
        : Result<RosterMembersContextResult, AppError>.Fail(
          AppError.BadRequest("Every roster program role and the Prepa role must exist."));
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
               roles.name AS role_name, COUNT(user_teams.su_id) AS member_count
          FROM discord.teams
          JOIN discord.servers USING (server_id)
          LEFT JOIN discord.roles USING (role_id)
          LEFT JOIN discord.user_teams USING (team_id)
         WHERE servers.guild_id = @GuildId
         GROUP BY teams.team_id, roles.name
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
