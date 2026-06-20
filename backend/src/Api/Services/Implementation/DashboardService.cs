using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Repositories;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed class DashboardService : IDashboardService
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly IDashboardRepository _repository;

  public DashboardService(
    IDbConnectionFactory connectionFactory,
    IDashboardRepository repository)
  {
    _connectionFactory = connectionFactory;
    _repository = repository;
  }

  public async Task<Result<DashboardContent, AppError>> GetDashboardContent(
    DashboardQuery query,
    CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenAsync(cancellationToken);
    var serversResult = await _repository.GetDiscordServers(connection, query, cancellationToken);

    return serversResult.Transform(servers => new DashboardContent
    {
      Servers = servers
    });
  }
}
