using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<Paged<Contact>, AppError>> GetContacts(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetContacts(connection, query);
  }

  public async Task<Result<Contact, AppError>> GetContact(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetContact(connection, id);
  }

  public async Task<Result<Contact, AppError>> CreateContact(ContactRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Contact, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.CreateContact(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Contact, AppError>> UpdateContact(int id, ContactRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Contact, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpdateContact(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteContact(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteContact(conn, tran, id))
      .Complete();
  }
}
