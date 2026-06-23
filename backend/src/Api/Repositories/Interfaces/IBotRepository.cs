using System.Data;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public interface IBotRepository
{
  // ==========================================================================
  // Guilds
  // ==========================================================================
  Task<Result<IReadOnlyCollection<GuildSummary>, AppError>> GetEnabledGuilds(IDbConnection connection);
  Task<Result<GuildProfile, AppError>> GetGuildProfile(IDbConnection connection, long guildId);
  Task<Result<GuildProfile, AppError>> UpdateGuildProfile(IDbConnection connection, IDbTransaction transaction, long guildId, GuildProfileRequest request);

  // ==========================================================================
  // Users and guild membership
  // ==========================================================================
  Task<Result<IReadOnlyCollection<BotUser>, AppError>> GetUsers(IDbConnection connection);
  Task<Result<BotUser, AppError>> CreateUser(IDbConnection connection, IDbTransaction transaction, BotUserRequest request);
  Task<Result<BotUser, AppError>> UpdateUser(IDbConnection connection, IDbTransaction transaction, int userId, BotUserRequest request);
  Task<Result<bool, AppError>> DeleteUser(IDbConnection connection, IDbTransaction transaction, int userId);
  Task<Result<IReadOnlyCollection<BotServerMember>, AppError>> GetGuildMembers(IDbConnection connection, long guildId);
  Task<Result<int, AppError>> InsertUser(IDbConnection connection, IDbTransaction transaction, string email, string fullname, string username);
  Task<Result<MemberVerification, AppError>> RegisterUserToGuild(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    RegisterGuildMemberRequest request);
  Task<Result<MemberVerification, AppError>> VerifyMember(IDbConnection connection, long guildId, VerifyMemberRequest request);
  Task<Result<MemberXp, AppError>> AddXp(IDbConnection connection, long guildId, XpRequest request);

  // ==========================================================================
  // Roles and channels
  // ==========================================================================
  Task<Result<IReadOnlyCollection<BotRole>, AppError>> GetGuildRoles(IDbConnection connection, long guildId);
  Task<Result<BotRole, AppError>> CreateGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, BotRoleRequest request);
  Task<Result<BotRole, AppError>> UpdateGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, int roleId, BotRoleRequest request);
  Task<Result<bool, AppError>> DeleteGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, int roleId);
  Task<Result<IReadOnlyCollection<BotChannel>, AppError>> GetGuildChannels(IDbConnection connection, long guildId);

  // ==========================================================================
  // Sync
  // ==========================================================================
  Task<Result<BotSyncResult, AppError>> SyncGuild(IDbConnection connection, IDbTransaction transaction, BotSyncRequest request);
}
