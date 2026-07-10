using System.Data;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public interface IInelicomRepository
{
  Task<Result<Paged<Contact>, AppError>> GetContactsByType(IDbConnection connection, string contactType, InelicomQuery query);

  Task<Result<Contact, AppError>> GetContactOfType(IDbConnection connection, string contactType, int id);



  Task<Result<Project, AppError>> GetProject(IDbConnection connection, int id);
  Task<Result<Paged<Project>, AppError>> GetProjects(IDbConnection connection, InelicomQuery query);

  Task<Result<Paged<Faculty>, AppError>> GetFaculties(IDbConnection connection, InelicomQuery query);
  Task<Result<Faculty, AppError>> GetFaculty(IDbConnection connection, int id);

  Task<Result<Paged<Building>, AppError>> GetBuildings(IDbConnection connection, InelicomQuery query);
  Task<Result<Building, AppError>> GetBuilding(IDbConnection connection, int id);

  Task<Result<Paged<Organization>, AppError>> GetOrganizations(IDbConnection connection, InelicomQuery query);
  Task<Result<Organization, AppError>> GetOrganization(IDbConnection connection, int id);

  Task<Result<CsvImportStats, AppError>> ImportCsv(
    IDbConnection connection,
    IDbTransaction transaction,
    string kind,
    string rowsJson);
}
