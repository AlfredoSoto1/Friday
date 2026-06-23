using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Services;

public interface IBotService
{
  Task<Result<IReadOnlyCollection<GuildSummary>, AppError>> GetEnabledGuilds();
  Task<Result<GuildProfile, AppError>> GetGuildProfile(long guildId);
  Task<Result<BotCommandResponse, AppError>> GetCommandResponse(long guildId, string commandName);
  Task<Result<MemberVerification, AppError>> VerifyMember(long guildId, VerifyMemberRequest request);
  Task<Result<MemberXp, AppError>> AddXp(long guildId, XpRequest request);
  Task<Result<BotSyncResult, AppError>> SyncGuild(BotSyncRequest request);
}
