using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<Paged<Department>, AppError>> GetDepartments(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetDepartments(connection, query);
  }

  public async Task<Result<Department, AppError>> GetDepartment(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetDepartment(connection, id);
  }

  public async Task<Result<Department, AppError>> CreateDepartment(DepartmentRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Department, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateDepartment(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Department, AppError>> UpdateDepartment(int id, DepartmentRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Department, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateDepartment(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteDepartment(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteDepartment(conn, tran, id))
      .Complete();
  }
}
