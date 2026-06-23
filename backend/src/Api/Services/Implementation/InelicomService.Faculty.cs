using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<Paged<Faculty>, AppError>> GetFaculties(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetFaculties(connection, query);
  }

  public async Task<Result<Faculty, AppError>> GetFaculty(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetFaculty(connection, id);
  }

  public async Task<Result<Faculty, AppError>> CreateFaculty(FacultyRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Faculty, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateFaculty(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Faculty, AppError>> UpdateFaculty(int id, FacultyRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Faculty, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateFaculty(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteFaculty(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteFaculty(conn, tran, id))
      .Complete();
  }
}
