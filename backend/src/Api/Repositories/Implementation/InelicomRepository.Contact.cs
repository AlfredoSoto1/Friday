using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<Paged<Contact>, AppError>> GetContacts(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT contact_id, name, email, phone, website, created_at, COUNT(*) OVER() AS total
          FROM inelicom.contacts
        WHERE @Search IS NULL OR name ILIKE @Search OR email ILIKE @Search
        ORDER BY name
        LIMIT @Limit OFFSET @Offset;
      ";

      var records = await connection.QueryAsync(sql, new
      {
        Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{query.Search}%",
        query.Limit,
        Offset = query.PageIndex * query.Limit
      });

      var items = records.Select(MapToContact).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Contact>, AppError>.Ok(new Paged<Contact>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Contact>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Contact, AppError>> GetContact(IDbConnection connection, int id)
  {
    try
    {
      const string sql = @"
        SELECT contact_id, name, email, phone, website, created_at
          FROM inelicom.contacts
        WHERE contact_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Contact, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Contact, AppError>.Ok(MapToContact(record));
    }
    catch (Exception ex)
    {
      return Result<Contact, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Contact, AppError>> CreateContact(IDbConnection connection, IDbTransaction transaction, ContactRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.contacts (name, email, phone, website)
        VALUES (@Name, @Email, @Phone, @Website)
        RETURNING contact_id, name, email, phone, website, created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Contact, AppError>.Ok(MapToContact(record));
    }
    catch (Exception ex)
    {
      return Result<Contact, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Contact, AppError>> UpdateContact(IDbConnection connection, IDbTransaction transaction, int id, ContactRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE inelicom.contacts
           SET name = @Name, email = @Email, phone = @Phone, website = @Website
        WHERE contact_id = @Id
        RETURNING contact_id, name, email, phone, website, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Name,
        request.Email,
        request.Phone,
        request.Website
      }, transaction);
      if (record is null)
      {
        return Result<Contact, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Contact, AppError>.Ok(MapToContact(record));
    }
    catch (Exception ex)
    {
      return Result<Contact, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteContact(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.contacts WHERE contact_id = @Id;";
      var affected = await connection.ExecuteAsync(sql, new { Id = id }, transaction);
      if (affected == 0)
      {
        return Result<bool, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static Contact MapToContact(dynamic record) => new()
  {
    ContactId = (int)record.contact_id,
    Name = (string)record.name,
    Email = (string)record.email,
    Phone = (string)record.phone,
    Website = (string)record.website,
    CreatedAt = (DateTime)record.created_at
  };
}
