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
  Task<Result<IReadOnlyCollection<BotServerMember>, AppError>> GetGuildMembers(IDbConnection connection, long guildId);
  Task<Result<MemberVerification, AppError>> VerifyMember(IDbConnection connection, long guildId, VerifyMemberRequest request);
  Task<Result<IReadOnlyCollection<RosterUserReference>, AppError>> UpsertRosterUsers(
    IDbConnection connection,
    IDbTransaction transaction,
    IReadOnlyCollection<RosterStudentAssignment> students);
  Task<Result<IReadOnlyCollection<RosterMemberReference>, AppError>> UpsertRosterMembers(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    IReadOnlyCollection<RosterUserReference> users);
  Task<Result<IReadOnlyCollection<RosterTeamReference>, AppError>> ReplaceRosterTeams(
    IDbConnection connection,
    IDbTransaction transaction,
    long guildId,
    IReadOnlyCollection<RosterTeamRequest> teams);
  Task<Result<RosterMembersContextResult, AppError>> ClearAutomaticRosterRoles(
    IDbConnection connection, IDbTransaction transaction, long guildId,
    IReadOnlyCollection<RosterUserReference> users,
    IReadOnlyCollection<RosterMemberReference> members);
  Task<Result<IReadOnlyCollection<GuildTeam>, AppError>> GetGuildTeams(
    IDbConnection connection, long guildId);
  Task<Result<int, AppError>> ReplaceRosterAssignments(
    IDbConnection connection,
    IDbTransaction transaction,
    IReadOnlyCollection<RosterStudentAssignment> students,
    IReadOnlyCollection<RosterUserReference> users,
    IReadOnlyCollection<RosterMemberReference> members,
    IReadOnlyCollection<RosterTeamReference> teams);
  Task<Result<MemberXp, AppError>> AddXp(IDbConnection connection, long guildId, XpRequest request);

  // ==========================================================================
  // Roles
  // ==========================================================================
  Task<Result<IReadOnlyCollection<BotRole>, AppError>> GetGuildRoles(IDbConnection connection, long guildId);
  Task<Result<BotRole, AppError>> CreateGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, BotRoleRequest request);
  Task<Result<BotRole, AppError>> UpdateGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, int roleId, BotRoleRequest request);
  Task<Result<bool, AppError>> DeleteGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, int roleId);

  // ==========================================================================
  // Sync
  // ==========================================================================
  Task<Result<BotSyncResult, AppError>> SyncGuild(IDbConnection connection, IDbTransaction transaction, BotSyncRequest request);
}
