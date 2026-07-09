using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  private const string ProjectColumns = @"
    project_id, web, facebook, instagram, email, name, description, created_at
  ";

  public async Task<Result<Paged<Project>, AppError>> GetProjects(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      var sql = $@"
        SELECT {ProjectColumns}, COUNT(*) OVER() AS total
          FROM inelicom.projects
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
      var sql = $@"
        SELECT {ProjectColumns}
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
      var sql = $@"
        INSERT INTO inelicom.projects (web, facebook, instagram, email, name, description)
        VALUES (@Web, @Facebook, @Instagram, @Email, @Name, @Description)
        RETURNING {ProjectColumns};
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
      var sql = $@"
        UPDATE inelicom.projects
           SET web = @Web,
               facebook = @Facebook,
               instagram = @Instagram,
               email = @Email,
               name = @Name,
               description = @Description
        WHERE project_id = @Id
        RETURNING {ProjectColumns};
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Web,
        request.Facebook,
        request.Instagram,
        request.Email,
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
    Web = record.web as string,
    Facebook = record.facebook as string,
    Instagram = record.instagram as string,
    Email = record.email as string,
    Name = (string)record.name,
    Description = (string)record.description,
    CreatedAt = (DateTime)record.created_at
  };
}
