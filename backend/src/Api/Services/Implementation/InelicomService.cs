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
  }}
