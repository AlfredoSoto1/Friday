using System.Data;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public interface IBotRepository
{
  Task<Result<IReadOnlyCollection<GuildSummary>, AppError>> GetEnabledGuilds(IDbConnection connection);
  Task<Result<GuildProfile, AppError>> GetGuildProfile(IDbConnection connection, long guildId);
  Task<Result<int, AppError>> InsertUser(IDbConnection connection, IDbTransaction transaction, string email, string fullname, string username);
  Task<Result<MemberVerification, AppError>> RegisterUserToGuild(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    string email,
    IReadOnlyCollection<string> discordRoleIds);
  Task<Result<MemberVerification, AppError>> VerifyMember(IDbConnection connection, long guildId, VerifyMemberRequest request);
  Task<Result<MemberXp, AppError>> AddXp(IDbConnection connection, long guildId, XpRequest request);
  Task<Result<BotSyncResult, AppError>> SyncGuild(IDbConnection connection, IDbTransaction transaction, BotSyncRequest request);
}
