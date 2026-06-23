using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Services;

public interface IDashboardService
{
  // ==========================================================================
  // Dashboard content
  // ==========================================================================
  Task<Result<DashboardContent, AppError>> GetDashboardContent(DashboardQuery query);

  // ==========================================================================
  // Runtime status and catalog
  // ==========================================================================
  Task<Result<BackendStatus, AppError>> GetStatus();
  Task<Result<CatalogSummary, AppError>> GetCatalogSummary();
}
