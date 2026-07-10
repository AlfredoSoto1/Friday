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

  public async Task<Result<DiscordServer, AppError>> CreateDiscordServer(CreateDiscordServerRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.Name)
      || string.IsNullOrWhiteSpace(request.ServerCode)
      || string.IsNullOrWhiteSpace(request.DepartmentProfile))
    {
      return Result<DiscordServer, AppError>.Fail(
        AppError.BadRequest("Server name, Discord server ID, and department are required."));
    }

    if (request.DepartmentProfile is not ("INEL_ICOM" or "INSO_CIIC"))
    {
      return Result<DiscordServer, AppError>.Fail(
        AppError.ValidationFailed("Department must be INEL/ICOM or INSO/CIIC."));
    }

    using var connection = _dbFactory.Create();
    return await _repository.CreateDiscordServer(connection, request);
  }

  public async Task<Result<DiscordServer, AppError>> SetDiscordServerEnabled(int serverId, bool enabled)
  {
    using var connection = _dbFactory.Create();
    return await _repository.SetDiscordServerEnabled(connection, serverId, enabled);
  }

  public async Task<Result<bool, AppError>> DeleteDiscordServer(int serverId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.DeleteDiscordServer(connection, serverId);
  }

  public async Task<Result<BackendStatus, AppError>> GetStatus()
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetStatus(connection);
  }

  public async Task<Result<CatalogSummary, AppError>> GetCatalogSummary()
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetCatalogSummary(connection);
  }

}
