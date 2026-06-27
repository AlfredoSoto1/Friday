using System.Data;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public interface IDashboardRepository
{
  // ==========================================================================
  // Dashboard content
  // ==========================================================================
  Task<Result<IReadOnlyCollection<DiscordServer>, AppError>> GetDiscordServers(IDbConnection connection, DashboardQuery query);
  Task<Result<DiscordServer, AppError>> CreateDiscordServer(
    IDbConnection connection,
    CreateDiscordServerRequest request);
  Task<Result<DiscordServer, AppError>> SetDiscordServerEnabled(
    IDbConnection connection,
    int serverId,
    bool enabled);
  Task<Result<bool, AppError>> DeleteDiscordServer(IDbConnection connection, int serverId);

  // ==========================================================================
  // Runtime status and catalog
  // ==========================================================================
  Task<Result<BackendStatus, AppError>> GetStatus(IDbConnection connection);
  Task<Result<CatalogSummary, AppError>> GetCatalogSummary(IDbConnection connection);
}
