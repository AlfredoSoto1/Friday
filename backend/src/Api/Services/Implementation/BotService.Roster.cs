using System.Data;
using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class BotService
{
  public async Task<Result<SaveGuildRosterResult, AppError>> SaveGuildRoster(
    long guildId,
    SaveGuildRosterRequest request)
  {
    var students = request.Teams.SelectMany(team =>
      team.Students.Select(student => new RosterStudentAssignment
      {
        Email = student.Email,
        FirstName = student.FirstName,
        FirstLastName = student.FirstLastName,
        SecondLastName = student.SecondLastName,
        Initial = student.Initial,
        Program = student.Program,
        TeamName = team.Name
      })).ToArray();

    if (request.Teams.Count == 0 || students.Length == 0)
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest("At least one team and student are required."));
    }

    var teamNames = request.Teams.Select(team => team.Name.Trim()).ToArray();
    var invalidStudents = students.Any(student =>
      string.IsNullOrWhiteSpace(student.Email) ||
      string.IsNullOrWhiteSpace(student.FirstName) ||
      string.IsNullOrWhiteSpace(student.FirstLastName) ||
      !new[] { "INEL", "ICOM", "INSO", "CIIC" }.Contains(student.Program) ||
      !teamNames.Contains(student.TeamName));
    var uniqueStudents =
      students.Select(student => student.Email).Distinct().Count();
    if (invalidStudents ||
        teamNames.Distinct().Count() != teamNames.Length ||
        uniqueStudents != students.Length)
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest(
          "Roster students and team names must be valid and unique."));
    }

    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<
        IReadOnlyCollection<RosterUserReference>, AppError>
      .Begin(connection, exception => AppError.BadRequest(exception.Message))
      .AndThen((conn, transaction) =>
        UpsertRosterUsers(conn, transaction, students))
      .AndThen((conn, transaction, users) =>
        UpsertRosterMembers(conn, transaction, guildId, users))
      .AndThen((conn, transaction, roster) =>
        ReplaceRosterTeams(conn, transaction, guildId, teamNames, roster))
      .AndThen((conn, transaction, roster) =>
        ReplaceRosterAssignments(conn, transaction, students, roster))
      .Complete();
  }

  private async Task<Result<IReadOnlyCollection<RosterUserReference>, AppError>>
    UpsertRosterUsers(
      IDbConnection connection,
      IDbTransaction transaction,
      IReadOnlyCollection<RosterStudentAssignment> students)
  {
    var result = await _repository.UpsertRosterUsers(
      connection, transaction, students);

    return result.IsSuccess && result.Value.Count != students.Count
      ? Result<IReadOnlyCollection<RosterUserReference>, AppError>.Fail(
        AppError.BadRequest("The complete student list could not be saved."))
      : result;
  }

  private async Task<Result<RosterMembersContext, AppError>> UpsertRosterMembers(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    IReadOnlyCollection<RosterUserReference> users)
  {
    var result = await _repository.UpsertRosterMembers(
      connection, transaction, guildId, users);

    if (result.IsFailure)
    {
      return Result<RosterMembersContext, AppError>.Fail(result.Error);
    }

    return result.Value.Count == users.Count
      ? Result<RosterMembersContext, AppError>.Ok(
        new RosterMembersContext(users, result.Value))
      : Result<RosterMembersContext, AppError>.Fail(
        AppError.NotFound($"Guild with ID {guildId} was not found."));
  }

  private async Task<Result<RosterTeamsContext, AppError>> ReplaceRosterTeams(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    IReadOnlyCollection<string> teamNames,
    RosterMembersContext roster)
  {
    var result = await _repository.ReplaceRosterTeams(
      connection, transaction, guildId, teamNames);

    if (result.IsFailure)
    {
      return Result<RosterTeamsContext, AppError>.Fail(result.Error);
    }

    return result.Value.Count == teamNames.Count
      ? Result<RosterTeamsContext, AppError>.Ok(
        new RosterTeamsContext(roster.Users, roster.Members, result.Value))
      : Result<RosterTeamsContext, AppError>.Fail(
        AppError.BadRequest("The complete team list could not be saved."));
  }

  private async Task<Result<SaveGuildRosterResult, AppError>>
    ReplaceRosterAssignments(
      IDbConnection connection,
      IDbTransaction transaction,
      IReadOnlyCollection<RosterStudentAssignment> students,
      RosterTeamsContext roster)
  {
    var result = await _repository.ReplaceRosterAssignments(
      connection,
      transaction,
      students,
      roster.Users,
      roster.Members,
      roster.Teams);

    if (result.IsFailure)
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(result.Error);
    }

    return result.Value == students.Count
      ? Result<SaveGuildRosterResult, AppError>.Ok(new SaveGuildRosterResult
      {
        StudentCount = result.Value,
        TeamCount = roster.Teams.Count
      })
      : Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest(
          "The complete student distribution could not be saved."));
  }

  private sealed record RosterMembersContext(
    IReadOnlyCollection<RosterUserReference> Users,
    IReadOnlyCollection<RosterMemberReference> Members);

  private sealed record RosterTeamsContext(
    IReadOnlyCollection<RosterUserReference> Users,
    IReadOnlyCollection<RosterMemberReference> Members,
    IReadOnlyCollection<RosterTeamReference> Teams);
}
