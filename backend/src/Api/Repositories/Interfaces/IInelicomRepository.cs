using System.Data;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public interface IInelicomRepository
{
  Task<Result<Paged<Contact>, AppError>> GetContacts(IDbConnection connection, InelicomQuery query);
  Task<Result<Contact, AppError>> GetContact(IDbConnection connection, int id);

  Task<Result<Paged<Faculty>, AppError>> GetFaculties(IDbConnection connection, InelicomQuery query);
  Task<Result<Faculty, AppError>> GetFaculty(IDbConnection connection, int id);

  Task<Result<Paged<Building>, AppError>> GetBuildings(IDbConnection connection, InelicomQuery query);
  Task<Result<Building, AppError>> GetBuilding(IDbConnection connection, int id);

  Task<Result<Paged<Department>, AppError>> GetDepartments(IDbConnection connection, InelicomQuery query);
  Task<Result<Department, AppError>> GetDepartment(IDbConnection connection, int id);

  Task<Result<Paged<Project>, AppError>> GetProjects(IDbConnection connection, InelicomQuery query);
  Task<Result<Project, AppError>> GetProject(IDbConnection connection, int id);

  Task<Result<Paged<Organization>, AppError>> GetOrganizations(IDbConnection connection, InelicomQuery query);
  Task<Result<Organization, AppError>> GetOrganization(IDbConnection connection, int id);


  Task<Result<int?, AppError>> GetFacultyIdByName(IDbConnection connection, IDbTransaction transaction, string name);
  Task<Result<int?, AppError>> GetBuildingIdByNameOrCode(IDbConnection connection, IDbTransaction transaction, string value);

  Task<Result<CsvImportStats, AppError>> ImportCsv(
    IDbConnection connection,
    IDbTransaction transaction,
    string kind,
    string rowsJson);
}
