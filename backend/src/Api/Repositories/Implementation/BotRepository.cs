using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class BotRepository : IBotRepository
{
  public async Task<Result<IReadOnlyCollection<GuildSummary>, AppError>> GetEnabledGuilds(IDbConnection connection)
  {
    try
    {
      const string sql = @"
        SELECT  name,
                guild_id,
                enabled,
                created_at
          FROM discord.servers
        ORDER BY name;
      ";

      var records = await connection.QueryAsync(sql);
      var guilds = records.Select(MapToGuildSummary).ToArray();

      return Result<IReadOnlyCollection<GuildSummary>, AppError>.Ok(guilds);
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<GuildSummary>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<GuildProfile, AppError>> GetGuildProfile(IDbConnection connection, long guildId)
  {
    try
    {
      const string sql = @"
        SELECT  name,
                guild_id,
                enabled,
                department_profile,
                primary_color,
                thumbnail_url,
                footer_text,
                verif_title,
                verif_desc,
                verif_button_id,
                verif_channel_id,
                verif_role_id,
                verif_banner_url,
                welcome_title,
                welcome_desc,
                welcome_channel_id,
                welcome_banner_url,
                created_at
          FROM discord.servers
        WHERE guild_id = @GuildId;
      ";

      var record = await connection.QueryFirstOrDefaultAsync(sql, new { GuildId = guildId.ToString() });
      return record is null
        ? Result<GuildProfile, AppError>.Fail(AppError.NotFound($"Guild with ID {guildId} not found."))
        : Result<GuildProfile, AppError>.Ok(MapToGuildProfile(record));
    }
    catch (Exception ex)
    {
      return Result<GuildProfile, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<MemberVerification, AppError>> VerifyMember(IDbConnection connection, long guildId, VerifyMemberRequest request)
  {
    try
    {
      const string sql = @"
        WITH verified_member AS (
          UPDATE discord.servers_users
             SET verified = TRUE,
                 funfact = COALESCE(@FunFact, funfact),
                 updated_at = CURRENT_TIMESTAMP
            FROM discord.servers, discord.users
          WHERE discord.servers.server_id = discord.servers_users.server_id
            AND discord.users.user_id = discord.servers_users.user_id
            AND discord.servers.guild_id = @GuildId
            AND discord.users.email = @Email
            AND discord.servers_users.discord_user_id = @DiscordUserId
          RETURNING discord.servers_users.su_id, discord.servers_users.verified
        )
        SELECT verified_member.verified,
               'Member verified successfully.' AS message,
               COALESCE(ARRAY_AGG(roles.discord_role_id ORDER BY roles.discord_role_id) FILTER (WHERE roles.discord_role_id IS NOT NULL), ARRAY[]::VARCHAR[]) AS role_ids
          FROM verified_member
            LEFT JOIN discord.user_roles ON user_roles.su_id = verified_member.su_id
            LEFT JOIN discord.roles ON roles.role_id = user_roles.role_id
        GROUP BY verified_member.su_id, verified_member.verified;
      ";

      var record = await connection.QueryFirstOrDefaultAsync(sql, new
      {
        GuildId = guildId.ToString(),
        request.DiscordUserId,
        request.Email,
        request.FunFact
      });

      return record is null
        ? Result<MemberVerification, AppError>.Fail(AppError.NotFound($"Member with ID {request.DiscordUserId} not found."))
        : Result<MemberVerification, AppError>.Ok(MapToVerifyMemberResult(record));
    }
    catch (Exception ex)
    {
      return Result<MemberVerification, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<MemberXp, AppError>> AddXp(IDbConnection connection, long guildId, XpRequest request)
  {
    try
    {
      const string sql = @"
        WITH current_member AS (
          SELECT servers_users.su_id, servers_users.level
            FROM discord.servers_users
              INNER JOIN discord.servers USING (server_id)
          WHERE discord.servers.guild_id = @GuildId
            AND servers_users.discord_user_id = @DiscordUserId
        ), updated_member AS (
          UPDATE discord.servers_users
             SET xp = discord.servers_users.xp + 1,
                 level = GREATEST(1, FLOOR(SQRT((discord.servers_users.xp + 1) / 25.0))::INT + 1),
                 updated_at = CURRENT_TIMESTAMP
            FROM current_member
          WHERE discord.servers_users.su_id = current_member.su_id
          RETURNING discord.servers_users.discord_user_id,
                    discord.servers_users.xp,
                    discord.servers_users.level,
                    discord.servers_users.level > current_member.level AS leveled_up
        )
        SELECT discord_user_id, xp, level, leveled_up
          FROM updated_member;
      ";

      var record = await connection.QueryFirstOrDefaultAsync(sql, new
      {
        GuildId = guildId.ToString(),
        request.DiscordUserId
      });

      return record is null
        ? Result<MemberXp, AppError>.Fail(AppError.NotFound($"Member with ID {request.DiscordUserId} not found."))
        : Result<MemberXp, AppError>.Ok(MapToXpResult(record));
    }
    catch (Exception ex)
    {
      return Result<MemberXp, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotSyncResult, AppError>> SyncGuild(
    IDbConnection connection,
    IDbTransaction transaction,
    BotSyncRequest request)
  {
    try
    {
      const string serverSql = @"
        SELECT server_id
          FROM discord.servers
         WHERE guild_id = @GuildId;
      ";

      var serverId = await connection.QuerySingleOrDefaultAsync<int>(serverSql, new
      {
        GuildId = request.GuildId.ToString()
      }, transaction);

      if (serverId == 0)
      {
        return Result<BotSyncResult, AppError>.Ok(new BotSyncResult
        {
          GuildId = request.GuildId,
          ServerRegistered = false
        });
      }

      const string upsertRolesSql = @"
        INSERT INTO discord.roles
          (server_id, discord_role_id, name, color, position, managed, mentionable, hoisted, updated_at)
        SELECT @ServerId,
               synced.discord_role_id,
               synced.name,
               synced.color,
               synced.position,
               synced.managed,
               synced.mentionable,
               synced.hoisted,
               CURRENT_TIMESTAMP
          FROM UNNEST(
            CAST(@DiscordRoleIds AS VARCHAR(32)[]),
            CAST(@RoleNames AS VARCHAR(255)[]),
            CAST(@RoleColors AS INT[]),
            CAST(@RolePositions AS INT[]),
            CAST(@RoleManaged AS BOOLEAN[]),
            CAST(@RoleMentionable AS BOOLEAN[]),
            CAST(@RoleHoisted AS BOOLEAN[]))
            AS synced(discord_role_id, name, color, position, managed, mentionable, hoisted)
        ON CONFLICT (server_id, discord_role_id)
        DO UPDATE SET name = EXCLUDED.name,
                      color = EXCLUDED.color,
                      position = EXCLUDED.position,
                      managed = EXCLUDED.managed,
                      mentionable = EXCLUDED.mentionable,
                      hoisted = EXCLUDED.hoisted,
                      updated_at = CURRENT_TIMESTAMP;
      ";

      var roleParameters = new
      {
        ServerId = serverId,
        DiscordRoleIds = request.Roles.Select(role => role.DiscordRoleId).ToArray(),
        RoleNames = request.Roles.Select(role => role.Name).ToArray(),
        RoleColors = request.Roles.Select(role => role.Color).ToArray(),
        RolePositions = request.Roles.Select(role => role.Position).ToArray(),
        RoleManaged = request.Roles.Select(role => role.Managed).ToArray(),
        RoleMentionable = request.Roles.Select(role => role.Mentionable).ToArray(),
        RoleHoisted = request.Roles.Select(role => role.Hoisted).ToArray()
      };

      await connection.ExecuteAsync(upsertRolesSql, roleParameters, transaction);

      const string deleteMissingRolesSql = @"
        DELETE FROM discord.roles
         WHERE server_id = @ServerId
           AND (discord_role_id IS NULL
                OR discord_role_id <> ALL(CAST(@DiscordRoleIds AS VARCHAR(32)[])));
      ";

      await connection.ExecuteAsync(deleteMissingRolesSql, roleParameters, transaction);
      var syncedAt = await connection.QuerySingleAsync<DateTime>(
        "SELECT CURRENT_TIMESTAMP;", transaction: transaction);

      return Result<BotSyncResult, AppError>.Ok(new BotSyncResult
      {
        GuildId = request.GuildId,
        RoleCount = request.Roles.Count,
        ServerRegistered = true,
        SyncedAt = syncedAt
      });
    }
    catch (Exception ex)
    {
      return Result<BotSyncResult, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static GuildSummary MapToGuildSummary(dynamic record) => new()
  {
    GuildId = long.Parse((string)record.guild_id),
    Name = (string)record.name,
    Enabled = (bool)record.enabled,
    CreatedAt = (DateTime)record.created_at
  };

  private static GuildProfile MapToGuildProfile(dynamic record) => new()
  {
    GuildId = long.Parse((string)record.guild_id),
    Name = (string)record.name,
    Enabled = (bool)record.enabled,
    DepartmentProfile = (string?)record.department_profile,
    CreatedAt = (DateTime)record.created_at,
    Theme = new Theme
    {
      PrimaryColor = (string)record.primary_color,
      ThumbnailUrl = (string?)record.thumbnail_url,
      FooterText = (string?)record.footer_text
    },
    Verification = new VerificationProfile
    {
      Title = (string)record.verif_title,
      Description = (string)record.verif_desc,
      ButtonLabel = (string)record.verif_button_id,
      ChannelId = (string?)record.verif_channel_id,
      VerifiedRoleId = (string?)record.verif_role_id,
      BannerUrl = (string?)record.verif_banner_url
    },
    Welcome = new WelcomeProfile
    {
      Title = (string)record.welcome_title,
      Description = (string)record.welcome_desc,
      ChannelId = (string?)record.welcome_channel_id,
      BannerUrl = (string?)record.welcome_banner_url
    }
  };

  private static MemberVerification MapToVerifyMemberResult(dynamic record) => new()
  {
    Verified = (bool)record.verified,
    Message = (string)record.message,
    RoleIds = ((IEnumerable<string>)record.role_ids).ToArray()
  };

  private static MemberXp MapToXpResult(dynamic record) => new()
  {
    DiscordUserId = (string)record.discord_user_id,
    Xp = (int)record.xp,
    Level = (int)record.level,
    LeveledUp = (bool)record.leveled_up
  };
}
