using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Repositories;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed class BotService : IBotService
{
  private readonly IDbConnectionFactory _dbFactory;
  private readonly IBotRepository _repository;

  public BotService(IDbConnectionFactory dbFactory, IBotRepository repository)
  {
    _dbFactory = dbFactory;
    _repository = repository;
  }

  public async Task<Result<IReadOnlyCollection<BotGuildSummary>, AppError>> GetEnabledGuilds()
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetEnabledGuilds(connection);
  }

  public async Task<Result<BotGuildProfile, AppError>> GetGuildProfile(long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildProfile(connection, guildId);
  }

  public async Task<Result<BotCommandResponse, AppError>> GetCommandResponse(long guildId, string commandName)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetCommandResponse(connection, guildId, commandName);
  }

  public async Task<Result<BotSetupProfile, AppError>> GetSetupProfile(long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetSetupProfile(connection, guildId);
  }

  public async Task<Result<BotVerifyMemberResult, AppError>> VerifyMember(long guildId, BotVerifyMemberRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.VerifyMember(connection, guildId, request);
  }

  public async Task<Result<BotXpResult, AppError>> AddXp(long guildId, BotXpRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.AddXp(connection, guildId, request);
  }

  public async Task<Result<BotSyncResult, AppError>> SyncGuild(BotSyncRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.SyncGuild(connection, request);
  }
}
