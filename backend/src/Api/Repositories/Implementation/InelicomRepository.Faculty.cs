using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  private const string FacultyColumns = @"
    faculty_id, name, extension, web, phone, facebook, email, office,
    job_entitlement, description, abbreviation, instagram, created_at
  ";

  public async Task<Result<Paged<Faculty>, AppError>> GetFaculties(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      var sql = $@"
        SELECT {FacultyColumns}, COUNT(*) OVER() AS total
          FROM inelicom.faculties
        WHERE @Search IS NULL OR name ILIKE @Search OR email ILIKE @Search OR description ILIKE @Search
        ORDER BY name
        LIMIT @Limit OFFSET @Offset;
      ";

      var records = await connection.QueryAsync(sql, new
      {
        Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{query.Search}%",
        query.Limit,
        Offset = query.PageIndex * query.Limit
      });

      var items = records.Select(MapToFaculty).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Faculty>, AppError>.Ok(new Paged<Faculty>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Faculty>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Faculty, AppError>> GetFaculty(IDbConnection connection, int id)
  {
    try
    {
      var sql = $@"
        SELECT {FacultyColumns}
          FROM inelicom.faculties
        WHERE faculty_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Faculty, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Faculty, AppError>.Ok(MapToFaculty(record));
    }
    catch (Exception ex)
    {
      return Result<Faculty, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Faculty, AppError>> CreateFaculty(IDbConnection connection, IDbTransaction transaction, FacultyRequest request)
  {
    try
    {
      var sql = $@"
        INSERT INTO inelicom.faculties
          (name, extension, web, phone, facebook, email, office, job_entitlement, description, abbreviation, instagram)
        VALUES
          (@Name, @Extension, @Web, @Phone, @Facebook, @Email, @Office, @JobEntitlement, @Description, @Abbreviation, @Instagram)
        RETURNING {FacultyColumns};
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Faculty, AppError>.Ok(MapToFaculty(record));
    }
    catch (Exception ex)
    {
      return Result<Faculty, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Faculty, AppError>> UpdateFaculty(IDbConnection connection, IDbTransaction transaction, int id, FacultyRequest request)
  {
    try
    {
      var sql = $@"
        UPDATE inelicom.faculties
           SET name = @Name,
               extension = @Extension,
               web = @Web,
               phone = @Phone,
               facebook = @Facebook,
               email = @Email,
               office = @Office,
               job_entitlement = @JobEntitlement,
               description = @Description,
               abbreviation = @Abbreviation,
               instagram = @Instagram
        WHERE faculty_id = @Id
        RETURNING {FacultyColumns};
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Name,
        request.Extension,
        request.Web,
        request.Phone,
        request.Facebook,
        request.Email,
        request.Office,
        request.JobEntitlement,
        request.Description,
        request.Abbreviation,
        request.Instagram
      }, transaction);
      if (record is null)
      {
        return Result<Faculty, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Faculty, AppError>.Ok(MapToFaculty(record));
    }
    catch (Exception ex)
    {
      return Result<Faculty, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteFaculty(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.faculties WHERE faculty_id = @Id;";
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

  private static Faculty MapToFaculty(dynamic record) => new()
  {
    FacultyId = (int)record.faculty_id,
    Name = (string)record.name,
    Extension = record.extension as string,
    Web = record.web as string,
    Phone = record.phone as string,
    Facebook = record.facebook as string,
    Email = record.email as string,
    Office = record.office as string,
    JobEntitlement = record.job_entitlement as string,
    Description = record.description as string,
    Abbreviation = record.abbreviation as string,
    Instagram = record.instagram as string,
    CreatedAt = (DateTime)record.created_at
  };
}
