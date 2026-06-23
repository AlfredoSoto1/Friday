using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Repositories;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed class InelicomService : IInelicomService
{
  private readonly IDbConnectionFactory _dbFactory;
  private readonly IInelicomRepository _repository;

  public InelicomService(IDbConnectionFactory dbFactory, IInelicomRepository repository)
  {
    _dbFactory = dbFactory;
    _repository = repository;
  }

  public async Task<Result<Paged<Contact>, AppError>> GetContacts(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetContacts(connection, query);
  }

  public async Task<Result<Contact, AppError>> GetContact(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetContact(connection, id);
  }

  public async Task<Result<Contact, AppError>> CreateContact(ContactRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.CreateContact(connection, request);
  }

  public async Task<Result<Contact, AppError>> UpdateContact(int id, ContactRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.UpdateContact(connection, id, request);
  }

  public async Task<Result<bool, AppError>> DeleteContact(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.DeleteContact(connection, id);
  }

  public async Task<Result<Paged<Faculty>, AppError>> GetFaculties(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetFaculties(connection, query);
  }

  public async Task<Result<Faculty, AppError>> GetFaculty(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetFaculty(connection, id);
  }

  public async Task<Result<Faculty, AppError>> CreateFaculty(FacultyRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.CreateFaculty(connection, request);
  }

  public async Task<Result<Faculty, AppError>> UpdateFaculty(int id, FacultyRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.UpdateFaculty(connection, id, request);
  }

  public async Task<Result<bool, AppError>> DeleteFaculty(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.DeleteFaculty(connection, id);
  }

  public async Task<Result<Paged<Building>, AppError>> GetBuildings(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetBuildings(connection, query);
  }

  public async Task<Result<Building, AppError>> GetBuilding(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetBuilding(connection, id);
  }

  public async Task<Result<Building, AppError>> CreateBuilding(BuildingRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.CreateBuilding(connection, request);
  }

  public async Task<Result<Building, AppError>> UpdateBuilding(int id, BuildingRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.UpdateBuilding(connection, id, request);
  }

  public async Task<Result<bool, AppError>> DeleteBuilding(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.DeleteBuilding(connection, id);
  }

  public async Task<Result<Paged<Department>, AppError>> GetDepartments(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetDepartments(connection, query);
  }

  public async Task<Result<Department, AppError>> GetDepartment(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetDepartment(connection, id);
  }

  public async Task<Result<Department, AppError>> CreateDepartment(DepartmentRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.CreateDepartment(connection, request);
  }

  public async Task<Result<Department, AppError>> UpdateDepartment(int id, DepartmentRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.UpdateDepartment(connection, id, request);
  }

  public async Task<Result<bool, AppError>> DeleteDepartment(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.DeleteDepartment(connection, id);
  }

  public async Task<Result<Paged<Room>, AppError>> GetRooms(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetRooms(connection, query);
  }

  public async Task<Result<Room, AppError>> GetRoom(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetRoom(connection, id);
  }

  public async Task<Result<Room, AppError>> CreateRoom(RoomRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.CreateRoom(connection, request);
  }

  public async Task<Result<Room, AppError>> UpdateRoom(int id, RoomRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.UpdateRoom(connection, id, request);
  }

  public async Task<Result<bool, AppError>> DeleteRoom(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.DeleteRoom(connection, id);
  }

  public async Task<Result<Paged<Project>, AppError>> GetProjects(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetProjects(connection, query);
  }

  public async Task<Result<Project, AppError>> GetProject(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetProject(connection, id);
  }

  public async Task<Result<Project, AppError>> CreateProject(ProjectRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.CreateProject(connection, request);
  }

  public async Task<Result<Project, AppError>> UpdateProject(int id, ProjectRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.UpdateProject(connection, id, request);
  }

  public async Task<Result<bool, AppError>> DeleteProject(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.DeleteProject(connection, id);
  }

  public async Task<Result<Paged<Organization>, AppError>> GetOrganizations(InelicomQuery query)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetOrganizations(connection, query);
  }

  public async Task<Result<Organization, AppError>> GetOrganization(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetOrganization(connection, id);
  }

  public async Task<Result<Organization, AppError>> CreateOrganization(OrganizationRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.CreateOrganization(connection, request);
  }

  public async Task<Result<Organization, AppError>> UpdateOrganization(int id, OrganizationRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.UpdateOrganization(connection, id, request);
  }

  public async Task<Result<bool, AppError>> DeleteOrganization(int id)
  {
    using var connection = _dbFactory.Create();
    return await _repository.DeleteOrganization(connection, id);
  }
}
