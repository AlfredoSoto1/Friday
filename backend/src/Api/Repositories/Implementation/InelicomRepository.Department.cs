using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<Paged<Department>, AppError>> GetDepartments(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT department_id, name, faculty_id, building_id, created_at, COUNT(*) OVER() AS total
          FROM inelicom.departments
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

      var items = records.Select(MapToDepartment).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Department>, AppError>.Ok(new Paged<Department>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Department>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Department, AppError>> GetDepartment(IDbConnection connection, int id)
  {
    try
    {
      const string sql = @"
        SELECT department_id, name, faculty_id, building_id, created_at
          FROM inelicom.departments
        WHERE department_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Department, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Department, AppError>.Ok(MapToDepartment(record));
    }
    catch (Exception ex)
    {
      return Result<Department, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Department, AppError>> CreateDepartment(IDbConnection connection, IDbTransaction transaction, DepartmentRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.departments (name, faculty_id, building_id)
        VALUES (@Name, @FacultyId, @BuildingId)
        RETURNING department_id, name, faculty_id, building_id, created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Department, AppError>.Ok(MapToDepartment(record));
    }
    catch (Exception ex)
    {
      return Result<Department, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Department, AppError>> UpdateDepartment(IDbConnection connection, IDbTransaction transaction, int id, DepartmentRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE inelicom.departments
           SET name = @Name, faculty_id = @FacultyId, building_id = @BuildingId
        WHERE department_id = @Id
        RETURNING department_id, name, faculty_id, building_id, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Name,
        request.FacultyId,
        request.BuildingId
      }, transaction);
      if (record is null)
      {
        return Result<Department, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Department, AppError>.Ok(MapToDepartment(record));
    }
    catch (Exception ex)
    {
      return Result<Department, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteDepartment(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.departments WHERE department_id = @Id;";
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

  private static Department MapToDepartment(dynamic record) => new()
  {
    DepartmentId = (int)record.department_id,
    Name = (string)record.name,
    FacultyId = (int)record.faculty_id,
    BuildingId = (int)record.building_id,
    CreatedAt = (DateTime)record.created_at
  };
}
