using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed class InelicomRepository : IInelicomRepository
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

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id, request.Name, request.Email, request.Phone, request.Website }, transaction);
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

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id, request.Name }, transaction);
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

  public async Task<Result<Paged<Building>, AppError>> GetBuildings(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT building_id, name, gpin, created_at, COUNT(*) OVER() AS total
          FROM inelicom.buildings
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

      var items = records.Select(MapToBuilding).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Building>, AppError>.Ok(new Paged<Building>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Building>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Building, AppError>> GetBuilding(IDbConnection connection, int id)
  {
    try
    {
      const string sql = @"
        SELECT building_id, name, gpin, created_at
          FROM inelicom.buildings
        WHERE building_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Building, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Building, AppError>.Ok(MapToBuilding(record));
    }
    catch (Exception ex)
    {
      return Result<Building, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Building, AppError>> CreateBuilding(IDbConnection connection, IDbTransaction transaction, BuildingRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.buildings (name, gpin)
        VALUES (@Name, @Gpin)
        RETURNING building_id, name, gpin, created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Building, AppError>.Ok(MapToBuilding(record));
    }
    catch (Exception ex)
    {
      return Result<Building, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Building, AppError>> UpdateBuilding(IDbConnection connection, IDbTransaction transaction, int id, BuildingRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE inelicom.buildings
           SET name = @Name, gpin = @Gpin
        WHERE building_id = @Id
        RETURNING building_id, name, gpin, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id, request.Name, request.Gpin }, transaction);
      if (record is null)
      {
        return Result<Building, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Building, AppError>.Ok(MapToBuilding(record));
    }
    catch (Exception ex)
    {
      return Result<Building, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteBuilding(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.buildings WHERE building_id = @Id;";
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

  private static Building MapToBuilding(dynamic record) => new()
  {
    BuildingId = (int)record.building_id,
    Name = (string)record.name,
    Gpin = (string)record.gpin,
    CreatedAt = (DateTime)record.created_at
  };

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

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id, request.Name, request.FacultyId, request.BuildingId }, transaction);
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

  public async Task<Result<Paged<Room>, AppError>> GetRooms(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT room_id, code, name, building_id, department_id, created_at, COUNT(*) OVER() AS total
          FROM inelicom.rooms
        WHERE @Search IS NULL OR code ILIKE @Search OR name ILIKE @Search
        ORDER BY code
        LIMIT @Limit OFFSET @Offset;
      ";

      var records = await connection.QueryAsync(sql, new
      {
        Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{query.Search}%",
        query.Limit,
        Offset = query.PageIndex * query.Limit
      });

      var items = records.Select(MapToRoom).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Room>, AppError>.Ok(new Paged<Room>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Room>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Room, AppError>> GetRoom(IDbConnection connection, int id)
  {
    try
    {
      const string sql = @"
        SELECT room_id, code, name, building_id, department_id, created_at
          FROM inelicom.rooms
        WHERE room_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Room, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Room, AppError>.Ok(MapToRoom(record));
    }
    catch (Exception ex)
    {
      return Result<Room, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Room, AppError>> CreateRoom(IDbConnection connection, IDbTransaction transaction, RoomRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.rooms (code, name, building_id, department_id)
        VALUES (@Code, @Name, @BuildingId, @DepartmentId)
        RETURNING room_id, code, name, building_id, department_id, created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Room, AppError>.Ok(MapToRoom(record));
    }
    catch (Exception ex)
    {
      return Result<Room, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Room, AppError>> UpdateRoom(IDbConnection connection, IDbTransaction transaction, int id, RoomRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE inelicom.rooms
           SET code = @Code, name = @Name, building_id = @BuildingId, department_id = @DepartmentId
        WHERE room_id = @Id
        RETURNING room_id, code, name, building_id, department_id, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id, request.Code, request.Name, request.BuildingId, request.DepartmentId }, transaction);
      if (record is null)
      {
        return Result<Room, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Room, AppError>.Ok(MapToRoom(record));
    }
    catch (Exception ex)
    {
      return Result<Room, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteRoom(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.rooms WHERE room_id = @Id;";
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

  private static Room MapToRoom(dynamic record) => new()
  {
    RoomId = (int)record.room_id,
    Code = (string)record.code,
    Name = (string)record.name,
    BuildingId = (int)record.building_id,
    DepartmentId = (int)record.department_id,
    CreatedAt = (DateTime)record.created_at
  };

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

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id, request.Name, request.Description }, transaction);
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

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id, request.Name, request.Description }, transaction);
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