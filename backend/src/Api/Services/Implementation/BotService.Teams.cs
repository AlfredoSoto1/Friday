using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class BotService
{
  public async Task<Result<IReadOnlyCollection<GuildTeam>, AppError>> GetGuildTeams(
    long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildTeams(connection, guildId);
  }
}
