using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<Paged<Project>, AppError>> GetProjects(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT project_id, name, description, created_at, COUNT(*) OVER() AS total
          FROM inelicom.projects
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

      var items = records.Select(MapToProject).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Project>, AppError>.Ok(new Paged<Project>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Project>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Project, AppError>> GetProject(IDbConnection connection, int id)
  {
    try
    {
      const string sql = @"
        SELECT project_id, name, description, created_at
          FROM inelicom.projects
        WHERE project_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Project, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Project, AppError>.Ok(MapToProject(record));
    }
    catch (Exception ex)
    {
      return Result<Project, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Project, AppError>> CreateProject(IDbConnection connection, IDbTransaction transaction, ProjectRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.projects (name, description)
        VALUES (@Name, @Description)
        RETURNING project_id, name, description, created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Project, AppError>.Ok(MapToProject(record));
    }
    catch (Exception ex)
    {
      return Result<Project, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Project, AppError>> UpdateProject(IDbConnection connection, IDbTransaction transaction, int id, ProjectRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE inelicom.projects
           SET name = @Name, description = @Description
        WHERE project_id = @Id
        RETURNING project_id, name, description, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Name,
        request.Description
      }, transaction);
      if (record is null)
      {
        return Result<Project, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Project, AppError>.Ok(MapToProject(record));
    }
    catch (Exception ex)
    {
      return Result<Project, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteProject(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.projects WHERE project_id = @Id;";
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

  private static Project MapToProject(dynamic record) => new()
  {
    ProjectId = (int)record.project_id,
    Name = (string)record.name,
    Description = (string)record.description,
    CreatedAt = (DateTime)record.created_at
  };
}
