using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Services;

public interface IDashboardService
{
  // ==========================================================================
  // Dashboard content
  // ==========================================================================
  Task<Result<DashboardContent, AppError>> GetDashboardContent(DashboardQuery query);
  Task<Result<DiscordServer, AppError>> CreateDiscordServer(CreateDiscordServerRequest request);
  Task<Result<DiscordServer, AppError>> SetDiscordServerEnabled(int serverId, bool enabled);
  Task<Result<bool, AppError>> DeleteDiscordServer(int serverId);

  // ==========================================================================
  // Runtime status and catalog
  // ==========================================================================
  Task<Result<BackendStatus, AppError>> GetStatus();
  Task<Result<CatalogSummary, AppError>> GetCatalogSummary();
}
