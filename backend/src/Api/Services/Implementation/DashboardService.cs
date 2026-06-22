using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Repositories;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed class DashboardService : IDashboardService
{
  private readonly IDbConnectionFactory _dbFactory;
  private readonly IDashboardRepository _repository;

  public DashboardService(IDbConnectionFactory dbFactory, IDashboardRepository repository)
  {
    _dbFactory = dbFactory;
    _repository = repository;
  }

  public async Task<Result<DashboardContent, AppError>> GetDashboardContent(DashboardQuery query)
  {
    using var connection = _dbFactory.Create();
    var serversResult = await _repository.GetDiscordServers(connection, query);

    return serversResult.Transform(servers => new DashboardContent
    {
      Servers = servers
    });
  }
}
