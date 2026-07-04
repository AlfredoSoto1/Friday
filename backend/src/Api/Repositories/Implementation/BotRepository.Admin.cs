using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class BotRepository
{
  public async Task<Result<IReadOnlyCollection<BotUser>, AppError>> GetUsers(IDbConnection connection)
  {
    try
    {
      const string sql = @"
        SELECT user_id, email, CONCAT_WS(' ', first_name, initial, first_last_name, second_last_name) AS fullname,
               SPLIT_PART(email, '@', 1) AS username, created_at
          FROM discord.users
        ORDER BY first_name, first_last_name, second_last_name;
      ";

      var records = await connection.QueryAsync(sql);
      return Result<IReadOnlyCollection<BotUser>, AppError>.Ok(records.Select(MapToBotUser).ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<BotUser>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotUser, AppError>> CreateUser(IDbConnection connection, IDbTransaction transaction, BotUserRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO discord.users (email, first_name)
        VALUES (@Email, @Fullname)
        RETURNING user_id, email,
                  first_name AS fullname,
                  SPLIT_PART(email, '@', 1) AS username,
                  created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<BotUser, AppError>.Ok(MapToBotUser(record));
    }
    catch (Exception ex)
    {
      return Result<BotUser, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotUser, AppError>> UpdateUser(IDbConnection connection, IDbTransaction transaction, int userId, BotUserRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE discord.users
           SET email = @Email,
               first_name = @Fullname
        WHERE user_id = @UserId
        RETURNING user_id, email,
                  CONCAT_WS(' ', first_name, initial, first_last_name, second_last_name) AS fullname,
                  SPLIT_PART(email, '@', 1) AS username,
                  created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        UserId = userId,
        request.Email,
        request.Fullname,
        request.Username
      }, transaction);

      return record is null
        ? Result<BotUser, AppError>.Fail(AppError.NotFound("User not found."))
        : Result<BotUser, AppError>.Ok(MapToBotUser(record));
    }
    catch (Exception ex)
    {
      return Result<BotUser, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteUser(IDbConnection connection, IDbTransaction transaction, int userId)
  {
    try
    {
      var affected = await connection.ExecuteAsync("DELETE FROM discord.users WHERE user_id = @UserId;", new { UserId = userId }, transaction);
      return affected == 0
        ? Result<bool, AppError>.Fail(AppError.NotFound("User not found."))
        : Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<IReadOnlyCollection<BotServerMember>, AppError>> GetGuildMembers(IDbConnection connection, long guildId)
  {
    try
    {
      const string sql = @"
        SELECT servers_users.su_id,
               users.user_id,
               users.email,
               CONCAT_WS(' ', users.first_name, users.initial, users.first_last_name, users.second_last_name) AS fullname,
               SPLIT_PART(users.email, '@', 1) AS username,
               servers_users.discord_user_id,
               servers_users.verified,
               servers_users.funfact,
               servers_users.xp,
               servers_users.level,
               servers_users.created_at,
               servers_users.updated_at,
               COALESCE(ARRAY_AGG(roles.discord_role_id ORDER BY roles.position DESC, roles.name) FILTER (WHERE roles.discord_role_id IS NOT NULL), ARRAY[]::VARCHAR[]) AS role_ids
          FROM discord.servers_users
            INNER JOIN discord.servers USING (server_id)
            INNER JOIN discord.users USING (user_id)
            LEFT JOIN discord.user_roles USING (su_id)
            LEFT JOIN discord.roles USING (role_id)
        WHERE discord.servers.guild_id = @GuildId
        GROUP BY servers_users.su_id, users.user_id, users.email, users.first_name,
                 users.initial, users.first_last_name, users.second_last_name,
                 servers_users.discord_user_id, servers_users.verified, servers_users.funfact,
                 servers_users.xp, servers_users.level, servers_users.created_at, servers_users.updated_at
        ORDER BY users.first_name, users.first_last_name, users.second_last_name;
      ";

      var records = await connection.QueryAsync(sql, new { GuildId = guildId.ToString() });
      return Result<IReadOnlyCollection<BotServerMember>, AppError>.Ok(records.Select(MapToBotServerMember).ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<BotServerMember>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<GuildProfile, AppError>> UpdateGuildProfile(IDbConnection connection, IDbTransaction transaction, long guildId, GuildProfileRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE discord.servers
           SET name = @Name,
               enabled = @Enabled,
               primary_color = @PrimaryColor,
               thumbnail_url = @ThumbnailUrl,
               footer_text = @FooterText,
               verif_title = @VerificationTitle,
               verif_desc = @VerificationDescription,
               verif_button_id = @VerificationButtonLabel,
               verif_channel_id = @VerificationChannelId,
               verif_role_id = @VerifiedRoleId,
               verif_banner_url = @VerificationBannerUrl,
               welcome_title = @WelcomeTitle,
               welcome_desc = @WelcomeDescription,
               welcome_channel_id = @WelcomeChannelId,
               welcome_banner_url = @WelcomeBannerUrl
        WHERE guild_id = @GuildId
        RETURNING name, guild_id, enabled, primary_color, thumbnail_url, footer_text,
                  verif_title, verif_desc, verif_button_id, verif_channel_id, verif_role_id, verif_banner_url,
                  welcome_title, welcome_desc, welcome_channel_id, welcome_banner_url, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        GuildId = guildId.ToString(),
        request.Name,
        request.Enabled,
        request.Theme.PrimaryColor,
        request.Theme.ThumbnailUrl,
        request.Theme.FooterText,
        VerificationTitle = request.Verification.Title,
        VerificationDescription = request.Verification.Description,
        VerificationButtonLabel = request.Verification.ButtonLabel,
        VerificationChannelId = request.Verification.ChannelId,
        VerifiedRoleId = request.Verification.VerifiedRoleId,
        VerificationBannerUrl = request.Verification.BannerUrl,
        WelcomeTitle = request.Welcome.Title,
        WelcomeDescription = request.Welcome.Description,
        WelcomeChannelId = request.Welcome.ChannelId,
        WelcomeBannerUrl = request.Welcome.BannerUrl
      }, transaction);

      return record is null
        ? Result<GuildProfile, AppError>.Fail(AppError.NotFound($"Guild with ID {guildId} not found."))
        : Result<GuildProfile, AppError>.Ok(MapToGuildProfile(record));
    }
    catch (Exception ex)
    {
      return Result<GuildProfile, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<IReadOnlyCollection<BotRole>, AppError>> GetGuildRoles(IDbConnection connection, long guildId)
  {
    try
    {
      const string sql = @"
        SELECT roles.role_id, servers.guild_id, roles.discord_role_id, roles.name, roles.color,
               roles.position, roles.managed, roles.mentionable, roles.hoisted, roles.created_at, roles.updated_at
          FROM discord.roles
            INNER JOIN discord.servers USING (server_id)
        WHERE servers.guild_id = @GuildId
        ORDER BY roles.position DESC NULLS LAST, roles.name;
      ";

      var records = await connection.QueryAsync(sql, new { GuildId = guildId.ToString() });
      return Result<IReadOnlyCollection<BotRole>, AppError>.Ok(records.Select(MapToBotRole).ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<BotRole>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotRole, AppError>> CreateGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, BotRoleRequest request)
  {
    try
    {
      const string sql = @"
        WITH target_server AS (
          SELECT server_id, guild_id FROM discord.servers WHERE guild_id = @GuildId
        )
        INSERT INTO discord.roles (server_id, discord_role_id, name, color, position, managed, mentionable, hoisted)
        SELECT server_id, @DiscordRoleId, @Name, @Color, @Position, @Managed, @Mentionable, @Hoisted
          FROM target_server
        RETURNING role_id, (SELECT guild_id FROM target_server) AS guild_id, discord_role_id, name, color, position, managed, mentionable, hoisted, created_at, updated_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        GuildId = guildId.ToString(),
        request.DiscordRoleId,
        request.Name,
        request.Color,
        request.Position,
        request.Managed,
        request.Mentionable,
        request.Hoisted
      }, transaction);

      return record is null
        ? Result<BotRole, AppError>.Fail(AppError.NotFound($"Guild with ID {guildId} not found."))
        : Result<BotRole, AppError>.Ok(MapToBotRole(record));
    }
    catch (Exception ex)
    {
      return Result<BotRole, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotRole, AppError>> UpdateGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, int roleId, BotRoleRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE discord.roles
           SET discord_role_id = @DiscordRoleId,
               name = @Name,
               color = @Color,
               position = @Position,
               managed = @Managed,
               mentionable = @Mentionable,
               hoisted = @Hoisted,
               updated_at = CURRENT_TIMESTAMP
          FROM discord.servers
        WHERE discord.roles.server_id = discord.servers.server_id
          AND discord.servers.guild_id = @GuildId
          AND discord.roles.role_id = @RoleId
        RETURNING discord.roles.role_id, discord.servers.guild_id, discord.roles.discord_role_id, discord.roles.name,
                  discord.roles.color, discord.roles.position, discord.roles.managed, discord.roles.mentionable,
                  discord.roles.hoisted, discord.roles.created_at, discord.roles.updated_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        GuildId = guildId.ToString(),
        RoleId = roleId,
        request.DiscordRoleId,
        request.Name,
        request.Color,
        request.Position,
        request.Managed,
        request.Mentionable,
        request.Hoisted
      }, transaction);

      return record is null
        ? Result<BotRole, AppError>.Fail(AppError.NotFound("Role not found."))
        : Result<BotRole, AppError>.Ok(MapToBotRole(record));
    }
    catch (Exception ex)
    {
      return Result<BotRole, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteGuildRole(IDbConnection connection, IDbTransaction transaction, long guildId, int roleId)
  {
    try
    {
      const string sql = @"
        DELETE FROM discord.roles
          USING discord.servers
        WHERE roles.server_id = servers.server_id
          AND servers.guild_id = @GuildId
          AND roles.role_id = @RoleId;
      ";

      var affected = await connection.ExecuteAsync(sql, new { GuildId = guildId.ToString(), RoleId = roleId }, transaction);
      return affected == 0
        ? Result<bool, AppError>.Fail(AppError.NotFound("Role not found."))
        : Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<IReadOnlyCollection<BotChannel>, AppError>> GetGuildChannels(IDbConnection connection, long guildId)
  {
    try
    {
      const string sql = @"
        SELECT channels.channel_id, servers.guild_id, channels.discord_channel_id, channels.parent_channel_id,
               channels.name, channels.type, channels.position, channels.topic, channels.nsfw,
               channels.created_at, channels.updated_at
          FROM discord.channels
            INNER JOIN discord.servers USING (server_id)
        WHERE servers.guild_id = @GuildId
        ORDER BY channels.position NULLS LAST, channels.name;
      ";

      var records = await connection.QueryAsync(sql, new { GuildId = guildId.ToString() });
      return Result<IReadOnlyCollection<BotChannel>, AppError>.Ok(records.Select(MapToBotChannel).ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<BotChannel>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }



  private static BotUser MapToBotUser(dynamic record) => new()
  {
    UserId = (int)record.user_id,
    Email = (string)record.email,
    Fullname = (string)record.fullname,
    Username = (string)record.username,
    CreatedAt = (DateTime)record.created_at
  };

  private static BotServerMember MapToBotServerMember(dynamic record) => new()
  {
    ServerUserId = (int)record.su_id,
    UserId = (int)record.user_id,
    Email = (string)record.email,
    Fullname = (string)record.fullname,
    Username = (string)record.username,
    DiscordUserId = (string)record.discord_user_id,
    Verified = (bool)record.verified,
    FunFact = (string?)record.funfact,
    Xp = (int)record.xp,
    Level = (int)record.level,
    RoleIds = ((IEnumerable<string>)record.role_ids).ToArray(),
    CreatedAt = (DateTime)record.created_at,
    UpdatedAt = (DateTime)record.updated_at
  };

  private static BotRole MapToBotRole(dynamic record) => new()
  {
    RoleId = (int)record.role_id,
    GuildId = long.Parse((string)record.guild_id),
    DiscordRoleId = (string?)record.discord_role_id,
    Name = (string)record.name,
    Color = (int?)record.color,
    Position = (int?)record.position,
    Managed = (bool)record.managed,
    Mentionable = (bool)record.mentionable,
    Hoisted = (bool)record.hoisted,
    CreatedAt = (DateTime)record.created_at,
    UpdatedAt = (DateTime)record.updated_at
  };

  private static BotChannel MapToBotChannel(dynamic record) => new()
  {
    ChannelId = (int)record.channel_id,
    GuildId = long.Parse((string)record.guild_id),
    DiscordChannelId = (string)record.discord_channel_id,
    ParentChannelId = (string?)record.parent_channel_id,
    Name = (string)record.name,
    Type = (string)record.type,
    Position = (int?)record.position,
    Topic = (string?)record.topic,
    Nsfw = (bool)record.nsfw,
    CreatedAt = (DateTime)record.created_at,
    UpdatedAt = (DateTime)record.updated_at
  };
}
