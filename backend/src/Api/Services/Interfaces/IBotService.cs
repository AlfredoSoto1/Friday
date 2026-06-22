using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Services;

public interface IBotService
{
  Task<Result<IReadOnlyCollection<BotGuildSummary>, AppError>> GetEnabledGuilds();
  Task<Result<BotGuildProfile, AppError>> GetGuildProfile(long guildId);
  Task<Result<BotCommandResponse, AppError>> GetCommandResponse(long guildId, string commandName);
  Task<Result<BotSetupProfile, AppError>> GetSetupProfile(long guildId);
  Task<Result<BotVerifyMemberResult, AppError>> VerifyMember(long guildId, BotVerifyMemberRequest request);
  Task<Result<BotXpResult, AppError>> AddXp(long guildId, BotXpRequest request);
}
