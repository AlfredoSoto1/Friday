using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<Paged<Organization>, AppError>> GetOrganizations(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT organization_id, name, description, created_at, COUNT(*) OVER() AS total
          FROM inelicom.organizations
        WHERE @Search IS NULL OR name ILIKE @Search OR description ILIKE @Search
        ORDER BY name
        LIMIT @Limit OFFSET @Offset;
      ";

      var records = await connection.QueryAsync(sql, new
      {
        Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{query.Search}%",
        query.Limit,
        Offset = query.PageIndex * query.Limit
      });

      var items = records.Select(MapToOrganization).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Organization>, AppError>.Ok(new Paged<Organization>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Organization>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Organization, AppError>> GetOrganization(IDbConnection connection, int id)
  {
    try
    {
      const string sql = @"
        SELECT organization_id, name, description, created_at
          FROM inelicom.organizations
        WHERE organization_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Organization, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Organization, AppError>.Ok(MapToOrganization(record));
    }
    catch (Exception ex)
    {
      return Result<Organization, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Organization, AppError>> CreateOrganization(IDbConnection connection, IDbTransaction transaction, OrganizationRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.organizations (name, description)
        VALUES (@Name, @Description)
        RETURNING organization_id, name, description, created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Organization, AppError>.Ok(MapToOrganization(record));
    }
    catch (Exception ex)
    {
      return Result<Organization, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Organization, AppError>> UpdateOrganization(IDbConnection connection, IDbTransaction transaction, int id, OrganizationRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE inelicom.organizations
           SET name = @Name, description = @Description
        WHERE organization_id = @Id
        RETURNING organization_id, name, description, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Name,
        request.Description
      }, transaction);
      if (record is null)
      {
        return Result<Organization, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Organization, AppError>.Ok(MapToOrganization(record));
    }
    catch (Exception ex)
    {
      return Result<Organization, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteOrganization(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.organizations WHERE organization_id = @Id;";
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

  private static Organization MapToOrganization(dynamic record) => new()
  {
    OrganizationId = (int)record.organization_id,
    Name = (string)record.name,
    Description = (string)record.description,
    CreatedAt = (DateTime)record.created_at
  };
}
