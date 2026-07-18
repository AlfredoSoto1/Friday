using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class BotRepository
{
  public async Task<Result<SaveGuildRosterResult, AppError>> SaveGuildRoster(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    SaveGuildRosterRequest request)
  {
    try
    {
      var teams = request.Teams.Select((team, index) => new
      {
        Index = index + 1,
        TeamId = team.TeamId ?? 0,
        Name = team.Name.Trim(),
        team.AppendMembers
      }).ToArray();
      var students = request.Teams.SelectMany((team, index) =>
        team.Students.Select(student => new
        {
          TeamIndex = index + 1,
          Email = student.Email.Trim(),
          FirstName = student.FirstName.Trim(),
          FirstLastName = student.FirstLastName.Trim(),
          SecondLastName = student.SecondLastName.Trim(),
          Initial = student.Initial.Trim()
        })).ToArray();
      var roles = request.Teams.SelectMany((team, index) =>
        team.RoleIds.Select(roleId => new
        {
          TeamIndex = index + 1,
          RoleId = roleId
        })).ToArray();

      const string sql = @"
        WITH input_teams AS MATERIALIZED (
          SELECT team_index,
                 NULLIF(team_id, 0) AS team_id,
                 BTRIM(name) AS name,
                 append_members
            FROM UNNEST(
              CAST(@TeamIndexes AS INTEGER[]),
              CAST(@TeamIds AS INTEGER[]),
              CAST(@TeamNames AS TEXT[]),
              CAST(@AppendMembers AS BOOLEAN[]))
              AS imported(team_index, team_id, name, append_members)
        ), input_students AS MATERIALIZED (
          SELECT team_index, email, first_name, first_last_name,
                 second_last_name, initial
            FROM UNNEST(
              CAST(@StudentTeamIndexes AS INTEGER[]),
              CAST(@Emails AS TEXT[]),
              CAST(@FirstNames AS TEXT[]),
              CAST(@FirstLastNames AS TEXT[]),
              CAST(@SecondLastNames AS TEXT[]),
              CAST(@Initials AS TEXT[]))
              AS imported(
                team_index, email, first_name, first_last_name,
                second_last_name, initial)
        ), input_roles AS MATERIALIZED (
          SELECT team_index, role_id
            FROM UNNEST(
              CAST(@RoleTeamIndexes AS INTEGER[]),
              CAST(@RoleIds AS INTEGER[]))
              AS imported(team_index, role_id)
        ), target_server AS MATERIALIZED (
          SELECT server_id
            FROM discord.servers
           WHERE guild_id = @GuildId
             FOR UPDATE
        ), validation AS MATERIALIZED (
          SELECT EXISTS(SELECT 1 FROM target_server) AS guild_exists,
                 NOT EXISTS (
                   SELECT 1
                     FROM input_teams
                    WHERE input_teams.team_id IS NOT NULL
                      AND NOT EXISTS (
                        SELECT 1
                          FROM discord.teams AS stored_teams
                          JOIN target_server
                            ON target_server.server_id = stored_teams.server_id
                         WHERE stored_teams.team_id = input_teams.team_id
                      )
                 ) AS teams_valid,
                 NOT EXISTS (
                   SELECT 1
                     FROM input_roles
                    WHERE NOT EXISTS (
                      SELECT 1
                        FROM discord.roles
                        JOIN target_server
                          ON target_server.server_id = roles.server_id
                       WHERE roles.role_id = input_roles.role_id
                    )
                 ) AS roles_valid
        ), all_valid AS MATERIALIZED (
          SELECT 1
            FROM validation
           WHERE guild_exists AND teams_valid AND roles_valid
        ), upserted_users AS (
          INSERT INTO discord.users AS stored_users (
            email, first_name, first_last_name, second_last_name, initial)
          SELECT input_students.email,
                 input_students.first_name,
                 input_students.first_last_name,
                 input_students.second_last_name,
                 input_students.initial
            FROM input_students
            CROSS JOIN all_valid
          ON CONFLICT (email)
          DO UPDATE SET first_name = EXCLUDED.first_name,
                        first_last_name = EXCLUDED.first_last_name,
                        second_last_name = EXCLUDED.second_last_name,
                        initial = EXCLUDED.initial
          RETURNING stored_users.user_id, stored_users.email
        ), upserted_members AS (
          INSERT INTO discord.servers_users AS stored_members (
            server_id, user_id)
          SELECT target_server.server_id, upserted_users.user_id
            FROM upserted_users
            CROSS JOIN target_server
            CROSS JOIN all_valid
          ON CONFLICT (server_id, user_id)
          DO UPDATE SET updated_at = CURRENT_TIMESTAMP
          RETURNING stored_members.su_id, stored_members.user_id
        ), max_team_position AS MATERIALIZED (
          SELECT COALESCE(MAX(stored_teams.position), 0) AS position
            FROM target_server
            LEFT JOIN discord.teams AS stored_teams
              ON stored_teams.server_id = target_server.server_id
        ), new_team_inputs AS MATERIALIZED (
          SELECT input_teams.team_index,
                 input_teams.name,
                 (max_team_position.position +
                   ROW_NUMBER() OVER (ORDER BY input_teams.team_index))::INTEGER
                   AS position
            FROM input_teams
            CROSS JOIN max_team_position
            CROSS JOIN all_valid
           WHERE input_teams.team_id IS NULL
        ), inserted_teams AS (
          INSERT INTO discord.teams (server_id, position, name)
          SELECT target_server.server_id,
                 new_team_inputs.position,
                 new_team_inputs.name
            FROM new_team_inputs
            CROSS JOIN target_server
          RETURNING team_id, position
        ), updated_teams AS (
          UPDATE discord.teams AS stored_teams
             SET name = input_teams.name,
                 updated_at = CURRENT_TIMESTAMP
            FROM input_teams, target_server, all_valid
           WHERE input_teams.team_id IS NOT NULL
             AND stored_teams.team_id = input_teams.team_id
             AND stored_teams.server_id = target_server.server_id
          RETURNING stored_teams.team_id
        ), resolved_teams AS MATERIALIZED (
          SELECT input_teams.team_index, updated_teams.team_id
            FROM input_teams
            JOIN updated_teams
              ON updated_teams.team_id = input_teams.team_id
          UNION ALL
          SELECT new_team_inputs.team_index, inserted_teams.team_id
            FROM new_team_inputs
            JOIN inserted_teams
              ON inserted_teams.position = new_team_inputs.position
        ), current_assignments AS MATERIALIZED (
          SELECT input_students.team_index,
                 upserted_members.su_id,
                 resolved_teams.team_id
            FROM input_students
            JOIN upserted_users
              ON upserted_users.email = input_students.email
            JOIN upserted_members
              ON upserted_members.user_id = upserted_users.user_id
            JOIN resolved_teams
              ON resolved_teams.team_index = input_students.team_index
        ), removed_team_memberships AS (
          DELETE FROM discord.user_teams AS stored_memberships
           USING resolved_teams, input_teams, all_valid
           WHERE resolved_teams.team_index = input_teams.team_index
             AND input_teams.append_members = FALSE
             AND stored_memberships.team_id = resolved_teams.team_id
             AND NOT EXISTS (
               SELECT 1
                 FROM current_assignments
                WHERE current_assignments.su_id = stored_memberships.su_id
                  AND current_assignments.team_id = stored_memberships.team_id
             )
          RETURNING stored_memberships.su_id
        ), inserted_team_memberships AS (
          INSERT INTO discord.user_teams (su_id, team_id)
          SELECT current_assignments.su_id, current_assignments.team_id
            FROM current_assignments
          ON CONFLICT (su_id, team_id) DO NOTHING
          RETURNING su_id
        ), assigned_roles AS (
          INSERT INTO discord.user_roles (su_id, role_id)
          SELECT current_assignments.su_id, input_roles.role_id
            FROM current_assignments
            JOIN input_roles
              ON input_roles.team_index = current_assignments.team_index
          ON CONFLICT (su_id, role_id) DO NOTHING
          RETURNING su_id
        )
        SELECT validation.guild_exists,
               validation.teams_valid,
               validation.roles_valid,
               (SELECT COUNT(*) FROM current_assignments)::INTEGER
                 AS student_count,
               (SELECT COUNT(*) FROM resolved_teams)::INTEGER AS team_count,
               (SELECT COUNT(*) FROM removed_team_memberships)::INTEGER
                 AS removed_membership_count,
               (SELECT COUNT(*) FROM inserted_team_memberships)::INTEGER
                 AS inserted_membership_count,
               (SELECT COUNT(*) FROM assigned_roles)::INTEGER
                 AS assigned_role_count
          FROM validation;
      ";
      var record = await connection.QuerySingleAsync(sql, new
      {
        GuildId = guildId.ToString(),
        TeamIndexes = teams.Select(team => team.Index).ToArray(),
        TeamIds = teams.Select(team => team.TeamId).ToArray(),
        TeamNames = teams.Select(team => team.Name).ToArray(),
        AppendMembers = teams.Select(team => team.AppendMembers).ToArray(),
        StudentTeamIndexes = students.Select(student => student.TeamIndex).ToArray(),
        Emails = students.Select(student => student.Email).ToArray(),
        FirstNames = students.Select(student => student.FirstName).ToArray(),
        FirstLastNames = students.Select(student => student.FirstLastName).ToArray(),
        SecondLastNames = students.Select(student => student.SecondLastName).ToArray(),
        Initials = students.Select(student => student.Initial).ToArray(),
        RoleTeamIndexes = roles.Select(role => role.TeamIndex).ToArray(),
        RoleIds = roles.Select(role => role.RoleId).ToArray()
      }, transaction);

      if (!(bool)record.guild_exists)
      {
        return Result<SaveGuildRosterResult, AppError>.Fail(
          AppError.NotFound($"Guild with ID {guildId} was not found."));
      }

      if (!(bool)record.teams_valid)
      {
        return Result<SaveGuildRosterResult, AppError>.Fail(
          AppError.NotFound(
            "One or more selected teams do not belong to this server."));
      }

      if (!(bool)record.roles_valid)
      {
        return Result<SaveGuildRosterResult, AppError>.Fail(
          AppError.BadRequest(
            "Each import group must use roles from this server."));
      }

      var studentCount = (int)record.student_count;
      var teamCount = (int)record.team_count;
      return studentCount == students.Length && teamCount == teams.Length
        ? Result<SaveGuildRosterResult, AppError>.Ok(new SaveGuildRosterResult
        {
          StudentCount = studentCount,
          TeamCount = teamCount
        })
        : Result<SaveGuildRosterResult, AppError>.Fail(
          AppError.BadRequest(
            "The complete roster distribution could not be saved."));
    }
    catch (Exception ex)
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest(ex.Message));
    }
  }
}
