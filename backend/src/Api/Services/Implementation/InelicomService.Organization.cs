using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<Paged<Organization>, AppError>> GetOrganizations(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetOrganizations(connection, query);
  }

  public async Task<Result<Organization, AppError>> GetOrganization(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetOrganization(connection, id);
  }

  public async Task<Result<Organization, AppError>> CreateOrganization(OrganizationRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Organization, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateOrganization(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Organization, AppError>> UpdateOrganization(int id, OrganizationRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Organization, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateOrganization(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteOrganization(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteOrganization(conn, tran, id))
      .Complete();
  }
}
