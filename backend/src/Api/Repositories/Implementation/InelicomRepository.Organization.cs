using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  private const string OrganizationColumns = @"
    organization_id, email, facebook, instagram, twitter_x, web, name, description, created_at
  ";

  public async Task<Result<Paged<Organization>, AppError>> GetOrganizations(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      var sql = $@"
        SELECT {OrganizationColumns}, COUNT(*) OVER() AS total
          FROM inelicom.organizations
        WHERE @Search IS NULL OR name ILIKE @Search OR description ILIKE @Search OR email ILIKE @Search
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
      var sql = $@"
        SELECT {OrganizationColumns}
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
      var sql = $@"
        INSERT INTO inelicom.organizations (email, facebook, instagram, twitter_x, web, name, description)
        VALUES (@Email, @Facebook, @Instagram, @TwitterX, @Web, @Name, @Description)
        RETURNING {OrganizationColumns};
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
      var sql = $@"
        UPDATE inelicom.organizations
           SET email = @Email,
               facebook = @Facebook,
               instagram = @Instagram,
               twitter_x = @TwitterX,
               web = @Web,
               name = @Name,
               description = @Description
        WHERE organization_id = @Id
        RETURNING {OrganizationColumns};
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Email,
        request.Facebook,
        request.Instagram,
        request.TwitterX,
        request.Web,
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
    Email = record.email as string,
    Facebook = record.facebook as string,
    Instagram = record.instagram as string,
    TwitterX = record.twitter_x as string,
    Web = record.web as string,
    Name = (string)record.name,
    Description = (string)record.description,
    CreatedAt = (DateTime)record.created_at
  };
}
