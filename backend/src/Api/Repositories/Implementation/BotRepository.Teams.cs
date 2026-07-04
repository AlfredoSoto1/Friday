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
        WITH program_roles AS (
          SELECT role_id, UPPER(name) AS program
            FROM discord.roles
            JOIN discord.servers USING (server_id)
           WHERE servers.guild_id = @GuildId
             AND UPPER(name) = ANY(@Programs)
        ), removed AS (
          DELETE FROM discord.user_roles
           USING program_roles
           WHERE user_roles.role_id = program_roles.role_id
             AND user_roles.su_id = ANY(@ServerUserIds)
        ), assigned AS (
          INSERT INTO discord.user_roles (su_id, role_id)
          SELECT members.su_id, program_roles.role_id
            FROM UNNEST(@Emails, @Programs) AS roster(email, program)
            JOIN discord.users ON users.email = roster.email
            JOIN discord.servers_users AS members USING (user_id)
            JOIN program_roles USING (program)
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
        ServerUserIds = members.Select(member => member.ServerUserId).ToArray()
      }, transaction);

      return count == students.Count
        ? Result<RosterMembersContextResult, AppError>.Ok(new()
        {
          Users = users,
          Members = members
        })
        : Result<RosterMembersContextResult, AppError>.Fail(
          AppError.BadRequest("INEL, ICOM, INSO, and CIIC roles must exist."));
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

  public async Task<Result<GuildTeam, AppError>> UpdateGuildTeam(
    IDbConnection connection, IDbTransaction transaction, long guildId,
    int teamId, UpdateGuildTeamRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE discord.teams
           SET name = @Name, role_id = @RoleId, updated_at = CURRENT_TIMESTAMP
          FROM discord.servers
         WHERE teams.server_id = servers.server_id
           AND servers.guild_id = @GuildId
           AND teams.team_id = @TeamId
        RETURNING teams.team_id, teams.position, teams.name, teams.role_id,
          (SELECT name FROM discord.roles WHERE role_id = teams.role_id) AS role_name,
          (SELECT COUNT(*) FROM discord.user_teams WHERE team_id = teams.team_id) AS member_count;
      ";
      var team = await connection.QuerySingleOrDefaultAsync<GuildTeam>(sql, new
      {
        GuildId = guildId.ToString(),
        TeamId = teamId,
        Name = request.Name.Trim(),
        request.RoleId
      }, transaction);
      return team is null
        ? Result<GuildTeam, AppError>.Fail(AppError.NotFound("Team not found."))
        : Result<GuildTeam, AppError>.Ok(team);
    }
    catch (Exception ex)
    {
      return Result<GuildTeam, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> ResetGuildTeams(
    IDbConnection connection, IDbTransaction transaction, long guildId)
  {
    const string sql = @"
      DELETE FROM discord.teams
       USING discord.servers
       WHERE teams.server_id = servers.server_id
         AND servers.guild_id = @GuildId;
    ";
    await connection.ExecuteAsync(
      sql, new { GuildId = guildId.ToString() }, transaction);
    return Result<bool, AppError>.Ok(true);
  }
}
