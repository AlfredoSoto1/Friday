using System.Data;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public interface IBotRepository
{
  Task<Result<IReadOnlyCollection<BotGuildSummary>, AppError>> GetEnabledGuilds(IDbConnection connection);
  Task<Result<BotGuildProfile, AppError>> GetGuildProfile(IDbConnection connection, long guildId);
  Task<Result<BotCommandResponse, AppError>> GetCommandResponse(IDbConnection connection, long guildId, string commandName);
  Task<Result<BotVerifyMemberResult, AppError>> VerifyMember(IDbConnection connection, long guildId, BotVerifyMemberRequest request);
  Task<Result<BotXpResult, AppError>> AddXp(IDbConnection connection, long guildId, BotXpRequest request);
  Task<Result<BotSyncResult, AppError>> SyncGuild(IDbConnection connection, BotSyncRequest request);
}
