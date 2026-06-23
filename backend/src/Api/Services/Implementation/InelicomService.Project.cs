using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<Paged<Project>, AppError>> GetProjects(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetProjects(connection, query);
  }

  public async Task<Result<Project, AppError>> GetProject(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetProject(connection, id);
  }

  public async Task<Result<Project, AppError>> CreateProject(ProjectRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Project, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateProject(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Project, AppError>> UpdateProject(int id, ProjectRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Project, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateProject(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteProject(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteProject(conn, tran, id))
      .Complete();
  }
}
