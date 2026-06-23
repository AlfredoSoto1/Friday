using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<Paged<Room>, AppError>> GetRooms(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetRooms(connection, query);
  }

  public async Task<Result<Room, AppError>> GetRoom(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetRoom(connection, id);
  }

  public async Task<Result<Room, AppError>> CreateRoom(RoomRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Room, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateRoom(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Room, AppError>> UpdateRoom(int id, RoomRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Room, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateRoom(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteRoom(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteRoom(conn, tran, id))
      .Complete();
  }
}
