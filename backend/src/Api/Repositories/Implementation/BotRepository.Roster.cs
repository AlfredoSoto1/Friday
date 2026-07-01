using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class BotRepository
{
  public async Task<Result<IReadOnlyCollection<RosterUserReference>, AppError>> UpsertRosterUsers(
    IDbConnection connection,
    IDbTransaction transaction,
    IReadOnlyCollection<RosterStudentRequest> students)
  {
    try
    {
      const string sql = @"
        INSERT INTO discord.users (email, fullname, username)
        SELECT email, fullname, username
          FROM UNNEST(@Emails, @Fullnames, @Usernames)
            AS roster(email, fullname, username)
        ON CONFLICT (email)
        DO UPDATE SET fullname = EXCLUDED.fullname,
                      username = EXCLUDED.username;

        SELECT user_id, email
          FROM discord.users
        WHERE email = ANY(@Emails);
      ";
      var records = await connection.QueryAsync(sql, new
      {
        Emails = students.Select(student => student.Email).ToArray(),
        Fullnames = students.Select(student => student.Fullname).ToArray(),
        Usernames = students.Select(student => student.Username).ToArray()
      }, transaction);
      var users = records.Select(record => new RosterUserReference
      {
        UserId = (int)record.user_id,
        Email = (string)record.email
      }).ToArray();

      return Result<IReadOnlyCollection<RosterUserReference>, AppError>.Ok(users);
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<RosterUserReference>, AppError>.Fail(
        AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<IReadOnlyCollection<RosterMemberReference>, AppError>> UpsertRosterMembers(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    IReadOnlyCollection<RosterUserReference> users)
  {
    try
    {
      const string sql = @"
        INSERT INTO discord.servers_users (server_id, user_id)
        SELECT servers.server_id, roster.user_id
          FROM discord.servers
          CROSS JOIN UNNEST(@UserIds) AS roster(user_id)
        WHERE servers.guild_id = @GuildId
        ON CONFLICT (server_id, user_id)
        DO UPDATE SET updated_at = CURRENT_TIMESTAMP
        RETURNING su_id, user_id;
      ";
      var records = await connection.QueryAsync(sql, new
      {
        GuildId = guildId.ToString(),
        UserIds = users.Select(user => user.UserId).ToArray()
      }, transaction);
      var members = records.Select(record => new RosterMemberReference
      {
        ServerUserId = (int)record.su_id,
        UserId = (int)record.user_id
      }).ToArray();

      return Result<IReadOnlyCollection<RosterMemberReference>, AppError>.Ok(members);
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<RosterMemberReference>, AppError>.Fail(
        AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<IReadOnlyCollection<RosterTeamReference>, AppError>> ReplaceRosterTeams(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    IReadOnlyCollection<string> teamNames)
  {
    try
    {
      const string deleteSql = @"
        DELETE FROM discord.teams
          USING discord.servers
        WHERE teams.server_id = servers.server_id
          AND servers.guild_id = @GuildId;
      ";
      await connection.ExecuteAsync(
        deleteSql,
        new { GuildId = guildId.ToString() },
        transaction);

      const string insertSql = @"
        INSERT INTO discord.teams (server_id, name)
        SELECT servers.server_id, roster.name
          FROM discord.servers
          CROSS JOIN UNNEST(@TeamNames) AS roster(name)
        WHERE servers.guild_id = @GuildId
        RETURNING team_id, name;
      ";
      var records = await connection.QueryAsync(insertSql, new
      {
        GuildId = guildId.ToString(),
        TeamNames = teamNames.ToArray()
      }, transaction);
      var teams = records.Select(record => new RosterTeamReference
      {
        TeamId = (int)record.team_id,
        Name = (string)record.name
      }).ToArray();

      return Result<IReadOnlyCollection<RosterTeamReference>, AppError>.Ok(teams);
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<RosterTeamReference>, AppError>.Fail(
        AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<int, AppError>> ReplaceRosterAssignments(
    IDbConnection connection,
    IDbTransaction transaction,
    IReadOnlyCollection<RosterStudentRequest> students,
    IReadOnlyCollection<RosterUserReference> users,
    IReadOnlyCollection<RosterMemberReference> members,
    IReadOnlyCollection<RosterTeamReference> teams)
  {
    try
    {
      var assignments = students
        .Join(users, student => student.Email, user => user.Email,
          (student, user) => new { student.TeamName, user.UserId })
        .Join(members, item => item.UserId, member => member.UserId,
          (item, member) => new { item.TeamName, member.ServerUserId })
        .Join(teams, item => item.TeamName, team => team.Name,
          (item, team) => new { item.ServerUserId, team.TeamId })
        .ToArray();
      const string sql = @"
        INSERT INTO discord.user_teams (su_id, team_id)
        SELECT su_id, team_id
          FROM UNNEST(@ServerUserIds, @TeamIds) AS roster(su_id, team_id)
        ON CONFLICT (su_id, team_id) DO NOTHING;
      ";
      var inserted = await connection.ExecuteAsync(sql, new
      {
        ServerUserIds = assignments.Select(item => item.ServerUserId).ToArray(),
        TeamIds = assignments.Select(item => item.TeamId).ToArray()
      }, transaction);

      return Result<int, AppError>.Ok(inserted);
    }
    catch (Exception ex)
    {
      return Result<int, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }
}
