using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Services;

public interface IBotService
{
  // ==========================================================================
  // Guilds
  // ==========================================================================
  Task<Result<IReadOnlyCollection<GuildSummary>, AppError>> GetEnabledGuilds();
  Task<Result<GuildProfile, AppError>> GetGuildProfile(long guildId);
  Task<Result<GuildProfile, AppError>> UpdateGuildProfile(long guildId, GuildProfileRequest request);

  // ==========================================================================
  // Users and guild membership
  // ==========================================================================
  Task<Result<IReadOnlyCollection<BotUser>, AppError>> GetUsers();
  Task<Result<BotUser, AppError>> CreateUser(BotUserRequest request);
  Task<Result<BotUser, AppError>> UpdateUser(int userId, BotUserRequest request);
  Task<Result<bool, AppError>> DeleteUser(int userId);
  Task<Result<IReadOnlyCollection<BotServerMember>, AppError>> GetGuildMembers(long guildId);
  Task<Result<MemberVerification, AppError>> RegisterUserToGuild(long guildId, RegisterGuildMemberRequest request);
  Task<Result<IReadOnlyCollection<MemberVerification>, AppError>> RegisterUsersToGuild(long guildId, BulkRegisterGuildMembersRequest request);
  Task<Result<MemberVerification, AppError>> VerifyMember(long guildId, VerifyMemberRequest request);
  Task<Result<IReadOnlyCollection<GuildTeam>, AppError>> GetGuildTeams(long guildId);
  Task<Result<GuildTeam, AppError>> UpdateGuildTeam(
    long guildId, int teamId, UpdateGuildTeamRequest request);
  Task<Result<bool, AppError>> ResetGuildTeams(long guildId);
  Task<Result<SaveGuildRosterResult, AppError>> SaveGuildRoster(
    long guildId,
    SaveGuildRosterRequest request);
  Task<Result<MemberXp, AppError>> AddXp(long guildId, XpRequest request);

  // ==========================================================================
  // Commands, roles, and channels
  // ==========================================================================
  Task<Result<BotCommandResponse, AppError>> GetCommandResponse(long guildId, string commandName);
  Task<Result<IReadOnlyCollection<BotRole>, AppError>> GetGuildRoles(long guildId);
  Task<Result<BotRole, AppError>> CreateGuildRole(long guildId, BotRoleRequest request);
  Task<Result<BotRole, AppError>> UpdateGuildRole(long guildId, int roleId, BotRoleRequest request);
  Task<Result<bool, AppError>> DeleteGuildRole(long guildId, int roleId);
  Task<Result<IReadOnlyCollection<BotChannel>, AppError>> GetGuildChannels(long guildId);

  // ==========================================================================
  // Sync
  // ==========================================================================
  Task<Result<BotSyncResult, AppError>> SyncGuild(BotSyncRequest request);
}
