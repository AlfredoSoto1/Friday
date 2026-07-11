using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class BotService
{
  public async Task<Result<GuildProfile, AppError>> UpdateGuildProfile(long guildId, GuildProfileRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<GuildProfile, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateGuildProfile(conn, tran, guildId, request))
      .Complete();
  }

  public async Task<Result<IReadOnlyCollection<BotUser>, AppError>> GetUsers()
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetUsers(connection);
  }

  public async Task<Result<IReadOnlyCollection<BotServerMember>, AppError>> GetGuildMembers(long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildMembers(connection, guildId);
  }

  public async Task<Result<IReadOnlyCollection<BotRole>, AppError>> GetGuildRoles(long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildRoles(connection, guildId);
  }

  public async Task<Result<BotRole, AppError>> CreateGuildRole(long guildId, BotRoleRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<BotRole, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateGuildRole(conn, tran, guildId, request))
      .Complete();
  }

  public async Task<Result<BotRole, AppError>> UpdateGuildRole(long guildId, int roleId, BotRoleRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<BotRole, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateGuildRole(conn, tran, guildId, roleId, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteGuildRole(long guildId, int roleId)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteGuildRole(conn, tran, guildId, roleId))
      .Complete();
  }

}
