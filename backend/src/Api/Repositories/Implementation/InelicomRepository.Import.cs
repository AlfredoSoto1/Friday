using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<bool, AppError>> UpsertBuilding(IDbConnection connection, IDbTransaction transaction, BuildingRequest request, ImportCounter counter)
  {
    try
    {
      const string existsSql = @"
        SELECT EXISTS (
          SELECT 1 FROM inelicom.buildings
          WHERE (@Code IS NOT NULL AND code = @Code) OR name = @Name
        );
      ";
      var existed = await connection.ExecuteScalarAsync<bool>(existsSql, request, transaction);

      const string sql = @"
        INSERT INTO inelicom.buildings (code, name, gpin)
        VALUES (@Code, @Name, @Gpin)
        ON CONFLICT (code) DO UPDATE
          SET name = EXCLUDED.name,
              gpin = EXCLUDED.gpin
        RETURNING building_id;
      ";

      await connection.ExecuteScalarAsync<int>(sql, request, transaction);
      if (existed) counter.AddUpdated();
      else counter.AddInserted();
      return Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> UpsertFaculty(IDbConnection connection, IDbTransaction transaction, FacultyRequest request, ImportCounter counter)
  {
    try
    {
      const string existsSql = "SELECT EXISTS (SELECT 1 FROM inelicom.faculties WHERE name = @Name);";
      var existed = await connection.ExecuteScalarAsync<bool>(existsSql, request, transaction);

      const string sql = @"
        INSERT INTO inelicom.faculties
          (name, extension, web, phone, facebook, email, office, job_entitlement, description, abbreviation, instagram)
        VALUES
          (@Name, @Extension, @Web, @Phone, @Facebook, @Email, @Office, @JobEntitlement, @Description, @Abbreviation, @Instagram)
        ON CONFLICT (name) DO UPDATE
          SET extension = EXCLUDED.extension,
              web = EXCLUDED.web,
              phone = EXCLUDED.phone,
              facebook = EXCLUDED.facebook,
              email = EXCLUDED.email,
              office = EXCLUDED.office,
              job_entitlement = EXCLUDED.job_entitlement,
              description = EXCLUDED.description,
              abbreviation = EXCLUDED.abbreviation,
              instagram = EXCLUDED.instagram
        RETURNING faculty_id;
      ";

      await connection.ExecuteScalarAsync<int>(sql, request, transaction);
      if (existed) counter.AddUpdated();
      else counter.AddInserted();
      return Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> UpsertProject(IDbConnection connection, IDbTransaction transaction, ProjectRequest request, ImportCounter counter)
  {
    try
    {
      const string existsSql = "SELECT EXISTS (SELECT 1 FROM inelicom.projects WHERE name = @Name);";
      var existed = await connection.ExecuteScalarAsync<bool>(existsSql, request, transaction);

      const string sql = @"
        INSERT INTO inelicom.projects (web, facebook, instagram, email, name, description)
        VALUES (@Web, @Facebook, @Instagram, @Email, @Name, @Description)
        ON CONFLICT (name) DO UPDATE
          SET web = EXCLUDED.web,
              facebook = EXCLUDED.facebook,
              instagram = EXCLUDED.instagram,
              email = EXCLUDED.email,
              description = EXCLUDED.description
        RETURNING project_id;
      ";

      await connection.ExecuteScalarAsync<int>(sql, request, transaction);
      if (existed) counter.AddUpdated();
      else counter.AddInserted();
      return Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> UpsertOrganization(IDbConnection connection, IDbTransaction transaction, OrganizationRequest request, ImportCounter counter)
  {
    try
    {
      const string existsSql = "SELECT EXISTS (SELECT 1 FROM inelicom.organizations WHERE name = @Name);";
      var existed = await connection.ExecuteScalarAsync<bool>(existsSql, request, transaction);

      const string sql = @"
        INSERT INTO inelicom.organizations (email, facebook, instagram, twitter_x, web, name, description)
        VALUES (@Email, @Facebook, @Instagram, @TwitterX, @Web, @Name, @Description)
        ON CONFLICT (name) DO UPDATE
          SET email = EXCLUDED.email,
              facebook = EXCLUDED.facebook,
              instagram = EXCLUDED.instagram,
              twitter_x = EXCLUDED.twitter_x,
              web = EXCLUDED.web,
              description = EXCLUDED.description
        RETURNING organization_id;
      ";

      await connection.ExecuteScalarAsync<int>(sql, request, transaction);
      if (existed) counter.AddUpdated();
      else counter.AddInserted();
      return Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }
}
