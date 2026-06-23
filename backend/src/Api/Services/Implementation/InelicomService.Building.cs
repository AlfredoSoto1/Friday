using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<Paged<Building>, AppError>> GetBuildings(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetBuildings(connection, query);
  }

  public async Task<Result<Building, AppError>> GetBuilding(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetBuilding(connection, id);
  }

  public async Task<Result<Building, AppError>> CreateBuilding(BuildingRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Building, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateBuilding(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Building, AppError>> UpdateBuilding(int id, BuildingRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Building, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateBuilding(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteBuilding(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteBuilding(conn, tran, id))
      .Complete();
  }
}
