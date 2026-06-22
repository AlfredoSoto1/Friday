using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Services;

public interface IDashboardService
{
  // ==========================================================================
  // ==========================================================================
  Task<Result<DashboardContent, AppError>> GetDashboardContent(DashboardQuery query);
}
