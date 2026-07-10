using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Repositories;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService : IInelicomService
{
  private readonly IDbConnectionFactory _dbFactory;
  private readonly IInelicomRepository _repository;

  public InelicomService(IDbConnectionFactory dbFactory, IInelicomRepository repository)
  {
    _dbFactory = dbFactory;
    _repository = repository;
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

}
