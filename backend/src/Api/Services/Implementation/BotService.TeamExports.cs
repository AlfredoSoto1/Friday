using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class BotService
{
  public async Task<Result<IReadOnlyCollection<PrepaTeamExportRow>, AppError>>
    GetGuildPrepaTeamExport(long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildPrepaTeamExport(connection, guildId);
  }
}
