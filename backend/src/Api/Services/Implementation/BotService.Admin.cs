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

  public async Task<Result<BotUser, AppError>> CreateUser(BotUserRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<BotUser, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateUser(conn, tran, request))
      .Complete();
  }

  public async Task<Result<BotUser, AppError>> UpdateUser(int userId, BotUserRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<BotUser, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateUser(conn, tran, userId, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteUser(int userId)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteUser(conn, tran, userId))
      .Complete();
  }

  public async Task<Result<IReadOnlyCollection<BotServerMember>, AppError>> GetGuildMembers(long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildMembers(connection, guildId);
  }

  public async Task<Result<MemberVerification, AppError>> RegisterUserToGuild(long guildId, RegisterGuildMemberRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<int, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.InsertUser(conn, tran, request.Email, request.Fullname, request.Username))
      .AndThen((conn, tran, _) => _repository.RegisterUserToGuild(conn, tran, guildId, request))
      .Complete();
  }

  public async Task<Result<IReadOnlyCollection<MemberVerification>, AppError>> RegisterUsersToGuild(long guildId, BulkRegisterGuildMembersRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    connection.Open();
    using var transaction = connection.BeginTransaction();
    var results = new List<MemberVerification>();

    try
    {
      foreach (var member in request.Members)
      {
        var userResult = await _repository.InsertUser(connection, transaction, member.Email, member.Fullname, member.Username);
        if (userResult.IsFailure)
        {
          await transaction.RollbackAsync();
          return Result<IReadOnlyCollection<MemberVerification>, AppError>.Fail(userResult.Error);
        }

        var registerResult = await _repository.RegisterUserToGuild(connection, transaction, guildId, member);
        if (registerResult.IsFailure)
        {
          await transaction.RollbackAsync();
          return Result<IReadOnlyCollection<MemberVerification>, AppError>.Fail(registerResult.Error);
        }

        results.Add(registerResult.Value);
      }

      await transaction.CommitAsync();
      return Result<IReadOnlyCollection<MemberVerification>, AppError>.Ok(results);
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      return Result<IReadOnlyCollection<MemberVerification>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
    finally
    {
      await connection.CloseAsync();
    }
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

  public async Task<Result<IReadOnlyCollection<BotChannel>, AppError>> GetGuildChannels(long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildChannels(connection, guildId);
  }

}
