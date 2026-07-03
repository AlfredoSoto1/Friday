using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class BotService
{
  public async Task<Result<IReadOnlyCollection<GuildTeam>, AppError>> GetGuildTeams(
    long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildTeams(connection, guildId);
  }

  public async Task<Result<GuildTeam, AppError>> UpdateGuildTeam(
    long guildId, int teamId, UpdateGuildTeamRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<GuildTeam, AppError>
      .Begin(connection, exception => AppError.BadRequest(exception.Message))
      .AndThen((conn, transaction) =>
        _repository.UpdateGuildTeam(conn, transaction, guildId, teamId, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> ResetGuildTeams(long guildId)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, exception => AppError.BadRequest(exception.Message))
      .AndThen((conn, transaction) =>
        _repository.ResetGuildTeams(conn, transaction, guildId))
      .Complete();
  }
}
