using System.Data;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public interface IInelicomRepository
{
  Task<Result<Paged<Contact>, AppError>> GetContacts(IDbConnection connection, InelicomQuery query);
  Task<Result<Contact, AppError>> GetContact(IDbConnection connection, int id);
  Task<Result<Contact, AppError>> CreateContact(IDbConnection connection, IDbTransaction transaction, ContactRequest request);
  Task<Result<Contact, AppError>> UpdateContact(IDbConnection connection, IDbTransaction transaction, int id, ContactRequest request);
  Task<Result<bool, AppError>> DeleteContact(IDbConnection connection, IDbTransaction transaction, int id);

  Task<Result<Paged<Faculty>, AppError>> GetFaculties(IDbConnection connection, InelicomQuery query);
  Task<Result<Faculty, AppError>> GetFaculty(IDbConnection connection, int id);
  Task<Result<Faculty, AppError>> CreateFaculty(IDbConnection connection, IDbTransaction transaction, FacultyRequest request);
  Task<Result<Faculty, AppError>> UpdateFaculty(IDbConnection connection, IDbTransaction transaction, int id, FacultyRequest request);
  Task<Result<bool, AppError>> DeleteFaculty(IDbConnection connection, IDbTransaction transaction, int id);

  Task<Result<Paged<Building>, AppError>> GetBuildings(IDbConnection connection, InelicomQuery query);
  Task<Result<Building, AppError>> GetBuilding(IDbConnection connection, int id);
  Task<Result<Building, AppError>> CreateBuilding(IDbConnection connection, IDbTransaction transaction, BuildingRequest request);
  Task<Result<Building, AppError>> UpdateBuilding(IDbConnection connection, IDbTransaction transaction, int id, BuildingRequest request);
  Task<Result<bool, AppError>> DeleteBuilding(IDbConnection connection, IDbTransaction transaction, int id);

  Task<Result<Paged<Department>, AppError>> GetDepartments(IDbConnection connection, InelicomQuery query);
  Task<Result<Department, AppError>> GetDepartment(IDbConnection connection, int id);
  Task<Result<Department, AppError>> CreateDepartment(IDbConnection connection, IDbTransaction transaction, DepartmentRequest request);
  Task<Result<Department, AppError>> UpdateDepartment(IDbConnection connection, IDbTransaction transaction, int id, DepartmentRequest request);
  Task<Result<bool, AppError>> DeleteDepartment(IDbConnection connection, IDbTransaction transaction, int id);

  Task<Result<Paged<Room>, AppError>> GetRooms(IDbConnection connection, InelicomQuery query);
  Task<Result<Room, AppError>> GetRoom(IDbConnection connection, int id);
  Task<Result<Room, AppError>> CreateRoom(IDbConnection connection, IDbTransaction transaction, RoomRequest request);
  Task<Result<Room, AppError>> UpdateRoom(IDbConnection connection, IDbTransaction transaction, int id, RoomRequest request);
  Task<Result<bool, AppError>> DeleteRoom(IDbConnection connection, IDbTransaction transaction, int id);

  Task<Result<Paged<Project>, AppError>> GetProjects(IDbConnection connection, InelicomQuery query);
  Task<Result<Project, AppError>> GetProject(IDbConnection connection, int id);
  Task<Result<Project, AppError>> CreateProject(IDbConnection connection, IDbTransaction transaction, ProjectRequest request);
  Task<Result<Project, AppError>> UpdateProject(IDbConnection connection, IDbTransaction transaction, int id, ProjectRequest request);
  Task<Result<bool, AppError>> DeleteProject(IDbConnection connection, IDbTransaction transaction, int id);

  Task<Result<Paged<Organization>, AppError>> GetOrganizations(IDbConnection connection, InelicomQuery query);
  Task<Result<Organization, AppError>> GetOrganization(IDbConnection connection, int id);
  Task<Result<Organization, AppError>> CreateOrganization(IDbConnection connection, IDbTransaction transaction, OrganizationRequest request);
  Task<Result<Organization, AppError>> UpdateOrganization(IDbConnection connection, IDbTransaction transaction, int id, OrganizationRequest request);
  Task<Result<bool, AppError>> DeleteOrganization(IDbConnection connection, IDbTransaction transaction, int id);


  Task<Result<bool, AppError>> UpsertBuilding(IDbConnection connection, IDbTransaction transaction, BuildingRequest request, ImportCounter counter);
  Task<Result<bool, AppError>> UpsertFaculty(IDbConnection connection, IDbTransaction transaction, FacultyRequest request, ImportCounter counter);
  Task<Result<bool, AppError>> UpsertProject(IDbConnection connection, IDbTransaction transaction, ProjectRequest request, ImportCounter counter);
  Task<Result<bool, AppError>> UpsertOrganization(IDbConnection connection, IDbTransaction transaction, OrganizationRequest request, ImportCounter counter);
}
