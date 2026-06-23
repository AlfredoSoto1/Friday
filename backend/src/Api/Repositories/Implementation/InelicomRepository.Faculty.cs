using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<Paged<Faculty>, AppError>> GetFaculties(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT faculty_id, name, created_at, COUNT(*) OVER() AS total
          FROM inelicom.faculties
        WHERE @Search IS NULL OR name ILIKE @Search
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
      const string sql = @"
        SELECT faculty_id, name, created_at
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
      const string sql = @"
        INSERT INTO inelicom.faculties (name)
        VALUES (@Name)
        RETURNING faculty_id, name, created_at;
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
      const string sql = @"
        UPDATE inelicom.faculties
           SET name = @Name
        WHERE faculty_id = @Id
        RETURNING faculty_id, name, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Name
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
    CreatedAt = (DateTime)record.created_at
  };
}
