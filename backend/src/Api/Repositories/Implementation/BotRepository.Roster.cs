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
    IReadOnlyCollection<RosterStudentAssignment> students)
  {
    try
    {
      const string sql = @"
        INSERT INTO discord.users (
          email, first_name, first_last_name, second_last_name, initial, program)
        SELECT email, first_name, first_last_name, second_last_name, initial, program
          FROM UNNEST(
            @Emails, @FirstNames, @FirstLastNames, @SecondLastNames, @Initials, @Programs)
          AS roster(
            email, first_name, first_last_name, second_last_name, initial, program)
        ON CONFLICT (email)
        DO UPDATE SET first_name = EXCLUDED.first_name,
                      first_last_name = EXCLUDED.first_last_name,
                      second_last_name = EXCLUDED.second_last_name,
                      initial = EXCLUDED.initial,
                      program = EXCLUDED.program;

        SELECT user_id, email
          FROM discord.users
        WHERE email = ANY(@Emails);
      ";
      var records = await connection.QueryAsync(sql, new
      {
        Emails = students.Select(student => student.Email).ToArray(),
        FirstNames = students.Select(student => student.FirstName).ToArray(),
        FirstLastNames = students.Select(student => student.FirstLastName).ToArray(),
        SecondLastNames = students.Select(student => student.SecondLastName).ToArray(),
        Initials = students.Select(student => student.Initial).ToArray(),
        Programs = students.Select(student => student.Program).ToArray()
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
    IReadOnlyCollection<RosterTeamRequest> teams)
  {
    try
    {
      var selectedTeamIds = teams
        .Where(team => team.TeamId is not null)
        .Select(team => team.TeamId!.Value)
        .ToArray();

      if (selectedTeamIds.Length > 0)
      {
        const string selectedTeamsSql = @"
          SELECT teams.team_id
            FROM discord.teams
            JOIN discord.servers USING (server_id)
           WHERE servers.guild_id = @GuildId
             AND teams.team_id = ANY(@TeamIds);
        ";
        var storedTeamIds = (await connection.QueryAsync<int>(selectedTeamsSql, new
        {
          GuildId = guildId.ToString(),
          TeamIds = selectedTeamIds
        }, transaction)).ToHashSet();

        if (storedTeamIds.Count != selectedTeamIds.Length)
        {
          return Result<IReadOnlyCollection<RosterTeamReference>, AppError>.Fail(
            AppError.NotFound("One or more selected teams do not belong to this server."));
        }
      }

      const string nextPositionSql = @"
        SELECT COALESCE(MAX(teams.position), 0)
          FROM discord.teams
          JOIN discord.servers USING (server_id)
         WHERE servers.guild_id = @GuildId;
      ";
      var nextPosition = await connection.QuerySingleAsync<int>(nextPositionSql, new
      {
        GuildId = guildId.ToString()
      }, transaction);
      var teamReferences = new List<RosterTeamReference>();

      foreach (var team in teams)
      {
        const string updateSql = @"
          UPDATE discord.teams
             SET name = @Name,
                 role_id = @RoleId,
                 updated_at = CURRENT_TIMESTAMP
            FROM discord.servers
            JOIN discord.roles ON roles.server_id = servers.server_id
           WHERE teams.team_id = @TeamId
             AND teams.server_id = servers.server_id
             AND servers.guild_id = @GuildId
             AND roles.role_id = @RoleId
          RETURNING teams.team_id, teams.name;
        ";
        const string insertSql = @"
          INSERT INTO discord.teams (server_id, position, name, role_id)
          SELECT servers.server_id, @Position, @Name, @RoleId
            FROM discord.servers
            JOIN discord.roles ON roles.server_id = servers.server_id
           WHERE servers.guild_id = @GuildId
             AND roles.role_id = @RoleId
          RETURNING team_id, name;
        ";
        var record = team.TeamId is null
          ? await connection.QuerySingleOrDefaultAsync(insertSql, new
          {
            GuildId = guildId.ToString(),
            Position = ++nextPosition,
            Name = team.Name.Trim(),
            RoleId = team.RoleId!.Value
          }, transaction)
          : await connection.QuerySingleOrDefaultAsync(updateSql, new
          {
            GuildId = guildId.ToString(),
            TeamId = team.TeamId.Value,
            Name = team.Name.Trim(),
            RoleId = team.RoleId!.Value
          }, transaction);

        if (record is null)
        {
          return Result<IReadOnlyCollection<RosterTeamReference>, AppError>.Fail(
            AppError.BadRequest("Each team must use a role from this server."));
        }

        teamReferences.Add(new RosterTeamReference
        {
          TeamId = (int)record.team_id,
          Name = (string)record.name,
          AppendMembers = team.AppendMembers
        });
      }

      return Result<IReadOnlyCollection<RosterTeamReference>, AppError>.Ok(teamReferences);
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
    IReadOnlyCollection<RosterStudentAssignment> students,
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
      var teamsToReplace = teams
        .Where(team => !team.AppendMembers)
        .Select(team => team.TeamId)
        .ToArray();
      if (teamsToReplace.Length > 0)
      {
        const string deleteSql = @"
          DELETE FROM discord.user_teams
           WHERE team_id = ANY(@TeamIds);
        ";
        await connection.ExecuteAsync(deleteSql, new
        {
          TeamIds = teamsToReplace
        }, transaction);
      }
      const string insertSql = @"
        INSERT INTO discord.user_teams (su_id, team_id)
        SELECT su_id, team_id
          FROM UNNEST(@ServerUserIds, @TeamIds) AS roster(su_id, team_id)
        ON CONFLICT (su_id, team_id) DO NOTHING;
      ";
      await connection.ExecuteAsync(insertSql, new
      {
        ServerUserIds = assignments.Select(item => item.ServerUserId).ToArray(),
        TeamIds = assignments.Select(item => item.TeamId).ToArray()
      }, transaction);

      return Result<int, AppError>.Ok(assignments.Length);
    }
    catch (Exception ex)
    {
      return Result<int, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }
}
