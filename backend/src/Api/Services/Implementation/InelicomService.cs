using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Repositories;
using Npgsql;
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
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Contact, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.CreateContact(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Contact, AppError>> UpdateContact(int id, ContactRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Contact, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.UpdateContact(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteContact(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.DeleteContact(conn, tran, id))
      .Complete();
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
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Faculty, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.CreateFaculty(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Faculty, AppError>> UpdateFaculty(int id, FacultyRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Faculty, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.UpdateFaculty(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteFaculty(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.DeleteFaculty(conn, tran, id))
      .Complete();
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
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Building, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.CreateBuilding(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Building, AppError>> UpdateBuilding(int id, BuildingRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Building, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.UpdateBuilding(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteBuilding(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.DeleteBuilding(conn, tran, id))
      .Complete();
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
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Department, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.CreateDepartment(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Department, AppError>> UpdateDepartment(int id, DepartmentRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Department, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.UpdateDepartment(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteDepartment(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.DeleteDepartment(conn, tran, id))
      .Complete();
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
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Room, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.CreateRoom(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Room, AppError>> UpdateRoom(int id, RoomRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Room, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.UpdateRoom(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteRoom(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.DeleteRoom(conn, tran, id))
      .Complete();
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
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Project, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.CreateProject(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Project, AppError>> UpdateProject(int id, ProjectRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Project, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.UpdateProject(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteProject(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.DeleteProject(conn, tran, id))
      .Complete();
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
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Organization, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.CreateOrganization(conn, tran, request))
      .Complete();
  }

  public async Task<Result<Organization, AppError>> UpdateOrganization(int id, OrganizationRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Organization, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.UpdateOrganization(conn, tran, id, request))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteOrganization(int id)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ToAppError)
      .AndThen((conn, tran) => _repository.DeleteOrganization(conn, tran, id))
      .Complete();
  }

  private static AppError ToAppError(Exception ex)
  {
    return AppError.BadRequest(ex.Message);
  }
}