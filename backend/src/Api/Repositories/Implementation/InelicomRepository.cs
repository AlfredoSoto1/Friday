using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed class InelicomRepository : IInelicomRepository
{
  public async Task<Result<Paged<Contact>, AppError>> GetContacts(IDbConnection connection, InelicomQuery query)
  {
    const string sql = @"
      SELECT contact_id, name, email, phone, website, created_at, COUNT(*) OVER() AS total
        FROM inelicom.contacts
      WHERE @Search IS NULL OR name ILIKE @Search OR email ILIKE @Search
      ORDER BY name
      LIMIT @Limit OFFSET @Offset;
    ";

    return await QueryPaged<Contact>(connection, sql, query);
  }

  public async Task<Result<Contact, AppError>> GetContact(IDbConnection connection, int id)
  {
    const string sql = @"
      SELECT contact_id, name, email, phone, website, created_at
        FROM inelicom.contacts
      WHERE contact_id = @Id;
    ";

    return await QuerySingle<Contact>(connection, sql, new { Id = id });
  }

  public async Task<Result<Contact, AppError>> CreateContact(IDbConnection connection, ContactRequest request)
  {
    const string sql = @"
      INSERT INTO inelicom.contacts (name, email, phone, website)
      VALUES (@Name, @Email, @Phone, @Website)
      RETURNING contact_id, name, email, phone, website, created_at;
    ";

    return await QuerySingle<Contact>(connection, sql, request);
  }

  public async Task<Result<Contact, AppError>> UpdateContact(IDbConnection connection, int id, ContactRequest request)
  {
    const string sql = @"
      UPDATE inelicom.contacts
         SET name = @Name, email = @Email, phone = @Phone, website = @Website
      WHERE contact_id = @Id
      RETURNING contact_id, name, email, phone, website, created_at;
    ";

    return await QuerySingle<Contact>(connection, sql, new { Id = id, request.Name, request.Email, request.Phone, request.Website });
  }

  public async Task<Result<bool, AppError>> DeleteContact(IDbConnection connection, int id)
  {
    const string sql = "DELETE FROM inelicom.contacts WHERE contact_id = @Id;";
    return await Delete(connection, sql, id);
  }

  public async Task<Result<Paged<Faculty>, AppError>> GetFaculties(IDbConnection connection, InelicomQuery query)
  {
    const string sql = @"
      SELECT faculty_id, name, created_at, COUNT(*) OVER() AS total
        FROM inelicom.faculties
      WHERE @Search IS NULL OR name ILIKE @Search
      ORDER BY name
      LIMIT @Limit OFFSET @Offset;
    ";

    return await QueryPaged<Faculty>(connection, sql, query);
  }

  public async Task<Result<Faculty, AppError>> GetFaculty(IDbConnection connection, int id)
  {
    const string sql = "SELECT faculty_id, name, created_at FROM inelicom.faculties WHERE faculty_id = @Id;";
    return await QuerySingle<Faculty>(connection, sql, new { Id = id });
  }

  public async Task<Result<Faculty, AppError>> CreateFaculty(IDbConnection connection, FacultyRequest request)
  {
    const string sql = "INSERT INTO inelicom.faculties (name) VALUES (@Name) RETURNING faculty_id, name, created_at;";
    return await QuerySingle<Faculty>(connection, sql, request);
  }

  public async Task<Result<Faculty, AppError>> UpdateFaculty(IDbConnection connection, int id, FacultyRequest request)
  {
    const string sql = "UPDATE inelicom.faculties SET name = @Name WHERE faculty_id = @Id RETURNING faculty_id, name, created_at;";
    return await QuerySingle<Faculty>(connection, sql, new { Id = id, request.Name });
  }

  public async Task<Result<bool, AppError>> DeleteFaculty(IDbConnection connection, int id)
  {
    const string sql = "DELETE FROM inelicom.faculties WHERE faculty_id = @Id;";
    return await Delete(connection, sql, id);
  }

  public async Task<Result<Paged<Building>, AppError>> GetBuildings(IDbConnection connection, InelicomQuery query)
  {
    const string sql = @"
      SELECT building_id, name, gpin, created_at, COUNT(*) OVER() AS total
        FROM inelicom.buildings
      WHERE @Search IS NULL OR name ILIKE @Search
      ORDER BY name
      LIMIT @Limit OFFSET @Offset;
    ";

    return await QueryPaged<Building>(connection, sql, query);
  }

  public async Task<Result<Building, AppError>> GetBuilding(IDbConnection connection, int id)
  {
    const string sql = "SELECT building_id, name, gpin, created_at FROM inelicom.buildings WHERE building_id = @Id;";
    return await QuerySingle<Building>(connection, sql, new { Id = id });
  }

  public async Task<Result<Building, AppError>> CreateBuilding(IDbConnection connection, BuildingRequest request)
  {
    const string sql = "INSERT INTO inelicom.buildings (name, gpin) VALUES (@Name, @Gpin) RETURNING building_id, name, gpin, created_at;";
    return await QuerySingle<Building>(connection, sql, request);
  }

  public async Task<Result<Building, AppError>> UpdateBuilding(IDbConnection connection, int id, BuildingRequest request)
  {
    const string sql = "UPDATE inelicom.buildings SET name = @Name, gpin = @Gpin WHERE building_id = @Id RETURNING building_id, name, gpin, created_at;";
    return await QuerySingle<Building>(connection, sql, new { Id = id, request.Name, request.Gpin });
  }

  public async Task<Result<bool, AppError>> DeleteBuilding(IDbConnection connection, int id)
  {
    const string sql = "DELETE FROM inelicom.buildings WHERE building_id = @Id;";
    return await Delete(connection, sql, id);
  }

  public async Task<Result<Paged<Department>, AppError>> GetDepartments(IDbConnection connection, InelicomQuery query)
  {
    const string sql = @"
      SELECT department_id, name, faculty_id, building_id, created_at, COUNT(*) OVER() AS total
        FROM inelicom.departments
      WHERE @Search IS NULL OR name ILIKE @Search
      ORDER BY name
      LIMIT @Limit OFFSET @Offset;
    ";

    return await QueryPaged<Department>(connection, sql, query);
  }

  public async Task<Result<Department, AppError>> GetDepartment(IDbConnection connection, int id)
  {
    const string sql = "SELECT department_id, name, faculty_id, building_id, created_at FROM inelicom.departments WHERE department_id = @Id;";
    return await QuerySingle<Department>(connection, sql, new { Id = id });
  }

  public async Task<Result<Department, AppError>> CreateDepartment(IDbConnection connection, DepartmentRequest request)
  {
    const string sql = "INSERT INTO inelicom.departments (name, faculty_id, building_id) VALUES (@Name, @FacultyId, @BuildingId) RETURNING department_id, name, faculty_id, building_id, created_at;";
    return await QuerySingle<Department>(connection, sql, request);
  }

  public async Task<Result<Department, AppError>> UpdateDepartment(IDbConnection connection, int id, DepartmentRequest request)
  {
    const string sql = "UPDATE inelicom.departments SET name = @Name, faculty_id = @FacultyId, building_id = @BuildingId WHERE department_id = @Id RETURNING department_id, name, faculty_id, building_id, created_at;";
    return await QuerySingle<Department>(connection, sql, new { Id = id, request.Name, request.FacultyId, request.BuildingId });
  }

  public async Task<Result<bool, AppError>> DeleteDepartment(IDbConnection connection, int id)
  {
    const string sql = "DELETE FROM inelicom.departments WHERE department_id = @Id;";
    return await Delete(connection, sql, id);
  }

  public async Task<Result<Paged<Room>, AppError>> GetRooms(IDbConnection connection, InelicomQuery query)
  {
    const string sql = @"
      SELECT room_id, code, name, building_id, department_id, created_at, COUNT(*) OVER() AS total
        FROM inelicom.rooms
      WHERE @Search IS NULL OR code ILIKE @Search OR name ILIKE @Search
      ORDER BY code
      LIMIT @Limit OFFSET @Offset;
    ";

    return await QueryPaged<Room>(connection, sql, query);
  }

  public async Task<Result<Room, AppError>> GetRoom(IDbConnection connection, int id)
  {
    const string sql = "SELECT room_id, code, name, building_id, department_id, created_at FROM inelicom.rooms WHERE room_id = @Id;";
    return await QuerySingle<Room>(connection, sql, new { Id = id });
  }

  public async Task<Result<Room, AppError>> CreateRoom(IDbConnection connection, RoomRequest request)
  {
    const string sql = "INSERT INTO inelicom.rooms (code, name, building_id, department_id) VALUES (@Code, @Name, @BuildingId, @DepartmentId) RETURNING room_id, code, name, building_id, department_id, created_at;";
    return await QuerySingle<Room>(connection, sql, request);
  }

  public async Task<Result<Room, AppError>> UpdateRoom(IDbConnection connection, int id, RoomRequest request)
  {
    const string sql = "UPDATE inelicom.rooms SET code = @Code, name = @Name, building_id = @BuildingId, department_id = @DepartmentId WHERE room_id = @Id RETURNING room_id, code, name, building_id, department_id, created_at;";
    return await QuerySingle<Room>(connection, sql, new { Id = id, request.Code, request.Name, request.BuildingId, request.DepartmentId });
  }

  public async Task<Result<bool, AppError>> DeleteRoom(IDbConnection connection, int id)
  {
    const string sql = "DELETE FROM inelicom.rooms WHERE room_id = @Id;";
    return await Delete(connection, sql, id);
  }

  public async Task<Result<Paged<Project>, AppError>> GetProjects(IDbConnection connection, InelicomQuery query)
  {
    const string sql = @"
      SELECT project_id, name, description, created_at, COUNT(*) OVER() AS total
        FROM inelicom.projects
      WHERE @Search IS NULL OR name ILIKE @Search OR description ILIKE @Search
      ORDER BY name
      LIMIT @Limit OFFSET @Offset;
    ";

    return await QueryPaged<Project>(connection, sql, query);
  }

  public async Task<Result<Project, AppError>> GetProject(IDbConnection connection, int id)
  {
    const string sql = "SELECT project_id, name, description, created_at FROM inelicom.projects WHERE project_id = @Id;";
    return await QuerySingle<Project>(connection, sql, new { Id = id });
  }

  public async Task<Result<Project, AppError>> CreateProject(IDbConnection connection, ProjectRequest request)
  {
    const string sql = "INSERT INTO inelicom.projects (name, description) VALUES (@Name, @Description) RETURNING project_id, name, description, created_at;";
    return await QuerySingle<Project>(connection, sql, request);
  }

  public async Task<Result<Project, AppError>> UpdateProject(IDbConnection connection, int id, ProjectRequest request)
  {
    const string sql = "UPDATE inelicom.projects SET name = @Name, description = @Description WHERE project_id = @Id RETURNING project_id, name, description, created_at;";
    return await QuerySingle<Project>(connection, sql, new { Id = id, request.Name, request.Description });
  }

  public async Task<Result<bool, AppError>> DeleteProject(IDbConnection connection, int id)
  {
    const string sql = "DELETE FROM inelicom.projects WHERE project_id = @Id;";
    return await Delete(connection, sql, id);
  }

  public async Task<Result<Paged<Organization>, AppError>> GetOrganizations(IDbConnection connection, InelicomQuery query)
  {
    const string sql = @"
      SELECT organization_id, name, description, created_at, COUNT(*) OVER() AS total
        FROM inelicom.organizations
      WHERE @Search IS NULL OR name ILIKE @Search OR description ILIKE @Search
      ORDER BY name
      LIMIT @Limit OFFSET @Offset;
    ";

    return await QueryPaged<Organization>(connection, sql, query);
  }

  public async Task<Result<Organization, AppError>> GetOrganization(IDbConnection connection, int id)
  {
    const string sql = "SELECT organization_id, name, description, created_at FROM inelicom.organizations WHERE organization_id = @Id;";
    return await QuerySingle<Organization>(connection, sql, new { Id = id });
  }

  public async Task<Result<Organization, AppError>> CreateOrganization(IDbConnection connection, OrganizationRequest request)
  {
    const string sql = "INSERT INTO inelicom.organizations (name, description) VALUES (@Name, @Description) RETURNING organization_id, name, description, created_at;";
    return await QuerySingle<Organization>(connection, sql, request);
  }

  public async Task<Result<Organization, AppError>> UpdateOrganization(IDbConnection connection, int id, OrganizationRequest request)
  {
    const string sql = "UPDATE inelicom.organizations SET name = @Name, description = @Description WHERE organization_id = @Id RETURNING organization_id, name, description, created_at;";
    return await QuerySingle<Organization>(connection, sql, new { Id = id, request.Name, request.Description });
  }

  public async Task<Result<bool, AppError>> DeleteOrganization(IDbConnection connection, int id)
  {
    const string sql = "DELETE FROM inelicom.organizations WHERE organization_id = @Id;";
    return await Delete(connection, sql, id);
  }

  private static async Task<Result<Paged<T>, AppError>> QueryPaged<T>(IDbConnection connection, string sql, InelicomQuery query)
  {
    try
    {
      var records = await connection.QueryAsync<T, long, (T Item, long Total)>(
        sql,
        (item, total) => (item, total),
        new
        {
          Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{query.Search}%",
          query.Limit,
          Offset = query.PageIndex * query.Limit
        },
        splitOn: "total");

      var items = records.Select(record => record.Item).ToArray();
      var total = records.Select(record => record.Total).FirstOrDefault();
      return Result<Paged<T>, AppError>.Ok(new Paged<T>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<T>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static async Task<Result<T, AppError>> QuerySingle<T>(IDbConnection connection, string sql, object parameters)
  {
    try
    {
      var record = await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
      return record is null
        ? Result<T, AppError>.Fail(AppError.NotFound("Record not found."))
        : Result<T, AppError>.Ok(record);
    }
    catch (Exception ex)
    {
      return Result<T, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static async Task<Result<bool, AppError>> Delete(IDbConnection connection, string sql, int id)
  {
    try
    {
      var affected = await connection.ExecuteAsync(sql, new { Id = id });
      return affected == 0
        ? Result<bool, AppError>.Fail(AppError.NotFound("Record not found."))
        : Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }
}
