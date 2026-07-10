using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository : IInelicomRepository
{
  public async Task<Result<Paged<Building>, AppError>> GetBuildings(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT building_id, code, name, gpin, created_at, COUNT(*) OVER() AS total
          FROM inelicom.buildings
        WHERE @Search IS NULL OR name ILIKE @Search OR code ILIKE @Search
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
        SELECT building_id, code, name, gpin, created_at
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

  private static Building MapToBuilding(dynamic record) => new()
  {
    BuildingId = (int)record.building_id,
    Code = record.code as string,
    Name = (string)record.name,
    Gpin = (string)record.gpin,
    CreatedAt = (DateTime)record.created_at
  };

  private static Contact MapToContact(dynamic record) => new()
  {
    ContactId = (int)record.contact_id,
    Name = (string)record.name,
    Email = (string)record.email,
    Phone = (string)record.phone,
    Website = (string)record.website,
    Description = record.description as string ?? "",
    Services = record.services as string ?? "",
    CreatedAt = (DateTime)record.created_at
  };

  public async Task<Result<Paged<Contact>, AppError>> GetContactsByType(IDbConnection connection, string contactType, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT contact_id, name, email, phone, website, created_at, COUNT(*) OVER() AS total
          FROM inelicom.contacts
        WHERE contact_type = @ContactType AND (@Search IS NULL OR name ILIKE @Search OR email ILIKE @Search)
        ORDER BY name
        LIMIT @Limit OFFSET @Offset;
      ";

      var records = await connection.QueryAsync(sql, new
      {
        ContactType = contactType,
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

  public async Task<Result<Contact, AppError>> GetContactOfType(IDbConnection connection, string contactType, int id)
  {
    try
    {
      const string sql = @"
        SELECT contact_id, name, email, phone, website, created_at
          FROM inelicom.contacts
        WHERE contact_type = @ContactType AND contact_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { ContactType = contactType, Id = id });
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

  public async Task<Result<Paged<Faculty>, AppError>> GetFaculties(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      var sql = $@"
        SELECT faculty_id, name, extension, web, phone, facebook, email, office,
          job_entitlement, description, abbreviation, instagram, created_at, COUNT(*) OVER() AS total
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
        SELECT faculty_id, name, extension, web, phone, facebook, email, office,
          job_entitlement, description, abbreviation, instagram, created_at
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

  private const string FacultyColumns = @"
    faculty_id, name, extension, web, phone, facebook, email, office,
    job_entitlement, description, abbreviation, instagram, created_at
  ";

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

  private static Organization MapToOrganization(dynamic record) => new()
  {
    OrganizationId = (int)record.organization_id,
    Name = (string)record.name,
    Description = record.description as string,
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

  private static Project MapToProject(dynamic record) => new()
  {
    ProjectId = (int)record.project_id,
    Name = (string)record.name,
    Description = record.description as string,
    CreatedAt = (DateTime)record.created_at
  };

}
