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
    var teams = request.Teams.ToArray();
    var students = teams.SelectMany(team => team.Students).ToArray();

    if (teams.Length == 0 || students.Length == 0)
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest("At least one team and student are required."));
    }

    var teamNames = teams.Select(team => team.Name.Trim()).ToArray();
    var selectedTeamIds = teams
      .Where(team => team.TeamId is not null)
      .Select(team => team.TeamId!.Value)
      .ToArray();
    var studentEmails = students
      .Select(student => student.Email.Trim())
      .ToArray();

    if (students.Any(student =>
      string.IsNullOrWhiteSpace(student.Email) ||
      string.IsNullOrWhiteSpace(student.FirstName) ||
      string.IsNullOrWhiteSpace(student.FirstLastName)) ||
      studentEmails.Distinct(StringComparer.OrdinalIgnoreCase).Count() !=
        studentEmails.Length)
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest("Roster students must be valid and unique."));
    }

    if (teamNames.Any(string.IsNullOrWhiteSpace) ||
        teamNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() !=
          teamNames.Length ||
        selectedTeamIds.Any(teamId => teamId <= 0) ||
        selectedTeamIds.Distinct().Count() != selectedTeamIds.Length)
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest(
          "Team names and existing team selections must be valid and unique."));
    }

    if (teams.Any(team =>
      team.RoleIds.Count == 0 ||
      team.RoleIds.Any(roleId => roleId <= 0) ||
      team.RoleIds.Distinct().Count() != team.RoleIds.Count))
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest(
          "Each import group must have at least one role without duplicate selections."));
    }

    try
    {
      using var connection = (NpgsqlConnection)_dbFactory.Create();
      return await TransactionResult<SaveGuildRosterResult, AppError>
        .Begin(connection, exception => AppError.BadRequest(exception.Message))
        .AndThen((conn, transaction) =>
          _repository.SaveGuildRoster(
            conn, transaction, guildId, request))
        .Complete();
    }
    catch (Exception ex)
    {
      return Result<SaveGuildRosterResult, AppError>.Fail(
        AppError.BadRequest(ex.Message));
    }
  }
}
