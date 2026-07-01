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

  public async Task<Result<int, AppError>> InsertUser(IDbConnection conn, IDbTransaction tx, string email, string fullname, string username)
  {
    try
    {
      const string sql = @"
        INSERT INTO discord.users (email, first_name)
        VALUES (@Email, @Fullname)
        ON CONFLICT (email)
        DO UPDATE SET first_name = EXCLUDED.first_name
        RETURNING user_id;
      ";

      var userId = await conn.QuerySingleAsync<int>(sql, new
      {
        Email = email,
        Fullname = fullname,
        Username = username
      }, tx);

      return Result<int, AppError>.Ok(userId);
    }
    catch (Exception ex)
    {
      return Result<int, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<MemberVerification, AppError>> RegisterUserToGuild(
    IDbConnection conn,
    IDbTransaction tx,
    long guildId,
    RegisterGuildMemberRequest request)
  {
    try
    {
      const string sql = @"
        WITH target_server AS (
          SELECT server_id
            FROM discord.servers
          WHERE guild_id = @GuildId
        ),
        target_user AS (
          SELECT user_id
            FROM discord.users
          WHERE email = @Email
        ),
        target_server_user AS (
          INSERT INTO discord.servers_users (server_id, user_id, discord_user_id, funfact, updated_at)
          SELECT target_server.server_id, target_user.user_id, @DiscordUserId, @FunFact, CURRENT_TIMESTAMP
            FROM target_server CROSS JOIN target_user
          ON CONFLICT (server_id, user_id)
          DO UPDATE SET discord_user_id = EXCLUDED.discord_user_id,
                        funfact = COALESCE(EXCLUDED.funfact, discord.servers_users.funfact),
                        updated_at = CURRENT_TIMESTAMP
          RETURNING su_id
        ), deleted_roles AS (
          DELETE FROM discord.user_roles
            USING target_server_user
          WHERE discord.user_roles.su_id = target_server_user.su_id
        ), selected_roles AS (
          SELECT roles.role_id, roles.discord_role_id
            FROM discord.roles
              INNER JOIN target_server USING (server_id)
          WHERE roles.discord_role_id = ANY(CAST(@DiscordRoleIds AS VARCHAR(32)[]))
        ), inserted_roles AS (
          INSERT INTO discord.user_roles (su_id, role_id)
          SELECT target_server_user.su_id, selected_roles.role_id
            FROM target_server_user, selected_roles
          ON CONFLICT DO NOTHING
          RETURNING role_id
        )
        SELECT FALSE AS verified,
               'Member registered successfully.' AS message,
               COALESCE(
                ARRAY_AGG(selected_roles.discord_role_id ORDER BY selected_roles.discord_role_id)
                  FILTER (WHERE selected_roles.discord_role_id IS NOT NULL), ARRAY[]::VARCHAR[]) AS role_ids
          FROM target_server_user
            LEFT JOIN selected_roles ON TRUE;
      ";

      var record = await conn.QueryFirstOrDefaultAsync(sql, new
      {
        GuildId = guildId.ToString(),
        request.Email,
        DiscordUserId = string.IsNullOrWhiteSpace(request.DiscordUserId) ? "-" : request.DiscordUserId,
        request.FunFact,
        DiscordRoleIds = request.DiscordRoleIds.ToArray()
      }, tx);

      return record is null
        ? Result<MemberVerification, AppError>.Fail(AppError.NotFound($"Guild or user not found for guild ID {guildId}."))
        : Result<MemberVerification, AppError>.Ok(MapToVerifyMemberResult(record));
    }
    catch (Exception ex)
    {
      return Result<MemberVerification, AppError>.Fail(AppError.BadRequest(ex.Message));
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
      const string sql = @"
        WITH target_server AS (
          INSERT INTO discord.servers (name, guild_id, enabled)
          VALUES (@GuildName, @GuildId, TRUE)
          ON CONFLICT (guild_id)
          DO UPDATE SET name = EXCLUDED.name,
                        enabled = TRUE
          RETURNING server_id
        ), synced_roles AS (
          INSERT INTO discord.roles
            (server_id, discord_role_id, name, color, position, managed, mentionable, hoisted, updated_at)
          SELECT target_server.server_id,
                 synced.discord_role_id,
                 synced.name,
                 synced.color,
                 synced.position,
                 synced.managed,
                 synced.mentionable,
                 synced.hoisted,
                 CURRENT_TIMESTAMP
            FROM target_server,
                 UNNEST(CAST(@DiscordRoleIds AS VARCHAR(32)[]), CAST(@RoleNames AS VARCHAR(255)[]), CAST(@RoleColors AS INT[]), CAST(@RolePositions AS INT[]), CAST(@RoleManaged AS BOOLEAN[]), CAST(@RoleMentionable AS BOOLEAN[]), CAST(@RoleHoisted AS BOOLEAN[]))
                   AS synced(discord_role_id, name, color, position, managed, mentionable, hoisted)
          ON CONFLICT (server_id, discord_role_id)
          DO UPDATE SET name = EXCLUDED.name,
                        color = EXCLUDED.color,
                        position = EXCLUDED.position,
                        managed = EXCLUDED.managed,
                        mentionable = EXCLUDED.mentionable,
                        hoisted = EXCLUDED.hoisted,
                        updated_at = CURRENT_TIMESTAMP
          RETURNING role_id
        ), synced_channels AS (
          INSERT INTO discord.channels
            (server_id, discord_channel_id, parent_channel_id, name, type, position, topic, nsfw, updated_at)
          SELECT target_server.server_id,
                 synced.discord_channel_id,
                 synced.parent_channel_id,
                 synced.name,
                 synced.type,
                 synced.position,
                 synced.topic,
                 synced.nsfw,
                 CURRENT_TIMESTAMP
            FROM target_server,
                 UNNEST(CAST(@DiscordChannelIds AS VARCHAR(32)[]), CAST(@ParentChannelIds AS VARCHAR(32)[]), CAST(@ChannelNames AS VARCHAR(255)[]), CAST(@ChannelTypes AS VARCHAR(50)[]), CAST(@ChannelPositions AS INT[]), CAST(@ChannelTopics AS TEXT[]), CAST(@ChannelNsfw AS BOOLEAN[]))
                   AS synced(discord_channel_id, parent_channel_id, name, type, position, topic, nsfw)
          ON CONFLICT (server_id, discord_channel_id)
          DO UPDATE SET parent_channel_id = EXCLUDED.parent_channel_id,
                        name = EXCLUDED.name,
                        type = EXCLUDED.type,
                        position = EXCLUDED.position,
                        topic = EXCLUDED.topic,
                        nsfw = EXCLUDED.nsfw,
                        updated_at = CURRENT_TIMESTAMP
          RETURNING channel_id
        ), sync_audit AS (
          INSERT INTO discord.server_syncs
            (server_id, role_count, channel_count, category_count, synced_by_discord_id)
          SELECT target_server.server_id, @RoleCount, @ChannelCount, @CategoryCount, @SyncedByDiscordId
            FROM target_server
          RETURNING synced_at
        )
        SELECT CAST(@GuildId AS BIGINT) AS guild_id,
               CAST(@RoleCount AS INT) AS role_count,
               CAST(@ChannelCount AS INT) AS channel_count,
               CAST(@CategoryCount AS INT) AS category_count,
               sync_audit.synced_at
          FROM sync_audit;
      ";

      var record = await connection.QuerySingleAsync(sql, new
      {
        GuildId = request.GuildId.ToString(),
        request.GuildName,
        RoleCount = request.Roles.Count,
        ChannelCount = request.Channels.Count,
        CategoryCount = request.Channels.Count(channel => string.Equals(channel.Type, "category", StringComparison.OrdinalIgnoreCase)),
        request.SyncedByDiscordId,
        DiscordRoleIds = request.Roles.Select(role => role.DiscordRoleId).ToArray(),
        RoleNames = request.Roles.Select(role => role.Name).ToArray(),
        RoleColors = request.Roles.Select(role => role.Color).ToArray(),
        RolePositions = request.Roles.Select(role => role.Position).ToArray(),
        RoleManaged = request.Roles.Select(role => role.Managed).ToArray(),
        RoleMentionable = request.Roles.Select(role => role.Mentionable).ToArray(),
        RoleHoisted = request.Roles.Select(role => role.Hoisted).ToArray(),
        DiscordChannelIds = request.Channels.Select(channel => channel.DiscordChannelId).ToArray(),
        ParentChannelIds = request.Channels.Select(channel => channel.ParentChannelId).ToArray(),
        ChannelNames = request.Channels.Select(channel => channel.Name).ToArray(),
        ChannelTypes = request.Channels.Select(channel => channel.Type).ToArray(),
        ChannelPositions = request.Channels.Select(channel => channel.Position).ToArray(),
        ChannelTopics = request.Channels.Select(channel => channel.Topic).ToArray(),
        ChannelNsfw = request.Channels.Select(channel => channel.Nsfw).ToArray()
      }, transaction);

      return Result<BotSyncResult, AppError>.Ok(new BotSyncResult
      {
        GuildId = request.GuildId,
        RoleCount = (int)record.role_count,
        ChannelCount = (int)record.channel_count,
        CategoryCount = (int)record.category_count,
        SyncedAt = (DateTime)record.synced_at
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
