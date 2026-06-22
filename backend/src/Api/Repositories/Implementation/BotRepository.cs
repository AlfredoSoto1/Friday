using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed class BotRepository : IBotRepository
{
  public async Task<Result<IReadOnlyCollection<BotGuildSummary>, AppError>> GetEnabledGuilds(IDbConnection connection)
  {
    try
    {
      const string sql = @"
        SELECT discord_guild_id, name, enabled
          FROM discord.bot_server_profiles
        WHERE enabled = TRUE
        ORDER BY name;
      ";

      var records = await connection.QueryAsync(sql);
      return Result<IReadOnlyCollection<BotGuildSummary>, AppError>.Ok(records.Select(MapGuildSummary).ToArray());
    }
    catch (Exception ex)
    {
      return Result<IReadOnlyCollection<BotGuildSummary>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotGuildProfile, AppError>> GetGuildProfile(IDbConnection connection, long guildId)
  {
    try
    {
      const string profileSql = @"
        SELECT discord_guild_id, name, enabled, primary_color, thumbnail_url, footer_text,
               verification_enabled, verification_title, verification_description,
               verification_button_label, verification_channel_id, verified_role_id,
               verification_banner_url, welcome_enabled, welcome_title,
               welcome_description, welcome_channel_id, welcome_banner_url
          FROM discord.bot_server_profiles
        WHERE discord_guild_id = @GuildId;
      ";

      var profile = await connection.QueryFirstOrDefaultAsync(profileSql, new { GuildId = guildId });
      var setup = await GetSetupProfile(connection, guildId);
      if (setup.IsFailure)
      {
        return Result<BotGuildProfile, AppError>.Fail(setup.Error);
      }

      if (profile is null)
      {
        return Result<BotGuildProfile, AppError>.Ok(DefaultProfile(guildId, setup.Value.Channels));
      }

      return Result<BotGuildProfile, AppError>.Ok(MapGuildProfile(profile, setup.Value.Channels));
    }
    catch (Exception ex)
    {
      return Result<BotGuildProfile, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotCommandResponse, AppError>> GetCommandResponse(IDbConnection connection, long guildId, string commandName)
  {
    try
    {
      const string sql = @"
        SELECT command_name, title, description, image_url, url, color, ephemeral
          FROM discord.bot_command_responses bcr
          INNER JOIN discord.bot_server_profiles bsp ON bsp.profile_id = bcr.profile_id
        WHERE bsp.discord_guild_id = @GuildId AND bcr.command_name = @CommandName;
      ";

      var response = await connection.QueryFirstOrDefaultAsync(sql, new { GuildId = guildId, CommandName = commandName });
      return Result<BotCommandResponse, AppError>.Ok(response is null
        ? DefaultCommandResponse(commandName)
        : MapCommandResponse(response));
    }
    catch (Exception ex)
    {
      return Result<BotCommandResponse, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotSetupProfile, AppError>> GetSetupProfile(IDbConnection connection, long guildId)
  {
    try
    {
      const string sql = @"
        SELECT bsc.name, bsc.type, bsc.category, bsc.position
          FROM discord.bot_setup_channels bsc
          INNER JOIN discord.bot_server_profiles bsp ON bsp.profile_id = bsc.profile_id
        WHERE bsp.discord_guild_id = @GuildId
        ORDER BY bsc.position, bsc.name;
      ";

      var records = await connection.QueryAsync(sql, new { GuildId = guildId });
      var channels = records.Select(MapSetupChannel).ToArray();
      return Result<BotSetupProfile, AppError>.Ok(new BotSetupProfile
      {
        Channels = channels.Length == 0 ? DefaultSetupChannels() : channels
      });
    }
    catch (Exception ex)
    {
      return Result<BotSetupProfile, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotVerifyMemberResult, AppError>> VerifyMember(
    IDbConnection connection,
    long guildId,
    BotVerifyMemberRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO discord.bot_member_verifications
          (discord_guild_id, discord_user_id, discord_username, email, fun_fact, verified)
        VALUES (@GuildId, @DiscordUserId, @DiscordUsername, @Email, @FunFact, TRUE)
        ON CONFLICT (discord_guild_id, discord_user_id)
        DO UPDATE SET email = EXCLUDED.email,
                      discord_username = EXCLUDED.discord_username,
                      fun_fact = EXCLUDED.fun_fact,
                      verified = TRUE,
                      verified_at = CURRENT_TIMESTAMP
        RETURNING verified;
      ";

      var verified = await connection.QuerySingleAsync<bool>(sql, new
      {
        GuildId = guildId,
        request.DiscordUserId,
        request.DiscordUsername,
        request.Email,
        request.FunFact
      });

      var profile = await GetGuildProfile(connection, guildId);
      var roleIds = profile.IsSuccess && profile.Value.Verification.VerifiedRoleId is not null
        ? new[] { profile.Value.Verification.VerifiedRoleId }
        : Array.Empty<string>();

      return Result<BotVerifyMemberResult, AppError>.Ok(new BotVerifyMemberResult
      {
        Verified = verified,
        Message = verified ? "Verification completed." : "Verification could not be completed.",
        RoleIds = roleIds
      });
    }
    catch (Exception ex)
    {
      return Result<BotVerifyMemberResult, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<BotXpResult, AppError>> AddXp(IDbConnection connection, long guildId, BotXpRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO discord.bot_member_levels
          (discord_guild_id, discord_user_id, discord_username, xp, level)
        VALUES (@GuildId, @DiscordUserId, @DiscordUsername, @Amount, 1)
        ON CONFLICT (discord_guild_id, discord_user_id)
        DO UPDATE SET xp = discord.bot_member_levels.xp + EXCLUDED.xp,
                      discord_username = EXCLUDED.discord_username,
                      level = GREATEST(1, FLOOR(SQRT((discord.bot_member_levels.xp + EXCLUDED.xp) / 25.0))::INT + 1),
                      updated_at = CURRENT_TIMESTAMP
        RETURNING discord_user_id, xp, level;
      ";

      var record = await connection.QuerySingleAsync(sql, new
      {
        GuildId = guildId,
        request.DiscordUserId,
        request.DiscordUsername,
        request.Amount
      });

      return Result<BotXpResult, AppError>.Ok(new BotXpResult
      {
        DiscordUserId = (string)record.discord_user_id,
        Xp = (int)record.xp,
        Level = (int)record.level,
        LeveledUp = request.Amount > 0 && (int)record.xp % 25 < request.Amount
      });
    }
    catch (Exception ex)
    {
      return Result<BotXpResult, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }


  public async Task<Result<BotSyncResult, AppError>> SyncGuild(IDbConnection connection, BotSyncRequest request)
  {
    if (connection is not System.Data.Common.DbConnection dbConnection)
    {
      return Result<BotSyncResult, AppError>.Fail(AppError.BadRequest("Sync requires a database connection."));
    }

    try
    {
      if (dbConnection.State != ConnectionState.Open)
      {
        await dbConnection.OpenAsync();
      }

      using var transaction = await dbConnection.BeginTransactionAsync();
      try
      {
        var serverId = await dbConnection.QuerySingleAsync<int>(@"
          INSERT INTO discord.servers (name, server_code)
          VALUES (@Name, @ServerCode)
          ON CONFLICT (server_code)
          DO UPDATE SET name = EXCLUDED.name
          RETURNING server_id;
        ", new
        {
          Name = request.GuildName,
          ServerCode = request.GuildId.ToString()
        }, transaction);

        await dbConnection.ExecuteAsync(@"
          INSERT INTO discord.bot_server_profiles (server_id, discord_guild_id, name)
          VALUES (@ServerId, @GuildId, @Name)
          ON CONFLICT (discord_guild_id)
          DO UPDATE SET server_id = EXCLUDED.server_id,
                        name = EXCLUDED.name,
                        updated_at = CURRENT_TIMESTAMP;
        ", new
        {
          ServerId = serverId,
          request.GuildId,
          Name = request.GuildName
        }, transaction);

        foreach (var role in request.Roles)
        {
          await dbConnection.ExecuteAsync(@"
            INSERT INTO discord.roles
              (server_id, discord_role_id, name, color, position, managed, mentionable, hoisted, updated_at)
            VALUES
              (@ServerId, @DiscordRoleId, @Name, @Color, @Position, @Managed, @Mentionable, @Hoisted, CURRENT_TIMESTAMP)
            ON CONFLICT (server_id, discord_role_id)
            DO UPDATE SET name = EXCLUDED.name,
                          color = EXCLUDED.color,
                          position = EXCLUDED.position,
                          managed = EXCLUDED.managed,
                          mentionable = EXCLUDED.mentionable,
                          hoisted = EXCLUDED.hoisted,
                          updated_at = CURRENT_TIMESTAMP;
          ", new
          {
            ServerId = serverId,
            role.DiscordRoleId,
            role.Name,
            role.Color,
            role.Position,
            role.Managed,
            role.Mentionable,
            role.Hoisted
          }, transaction);
        }

        foreach (var channel in request.Channels)
        {
          await dbConnection.ExecuteAsync(@"
            INSERT INTO discord.channels
              (server_id, discord_channel_id, parent_channel_id, name, type, position, topic, nsfw, updated_at)
            VALUES
              (@ServerId, @DiscordChannelId, @ParentChannelId, @Name, @Type, @Position, @Topic, @Nsfw, CURRENT_TIMESTAMP)
            ON CONFLICT (server_id, discord_channel_id)
            DO UPDATE SET parent_channel_id = EXCLUDED.parent_channel_id,
                          name = EXCLUDED.name,
                          type = EXCLUDED.type,
                          position = EXCLUDED.position,
                          topic = EXCLUDED.topic,
                          nsfw = EXCLUDED.nsfw,
                          updated_at = CURRENT_TIMESTAMP;
          ", new
          {
            ServerId = serverId,
            channel.DiscordChannelId,
            channel.ParentChannelId,
            channel.Name,
            channel.Type,
            channel.Position,
            channel.Topic,
            channel.Nsfw
          }, transaction);
        }

        var categoryCount = request.Channels.Count(channel => string.Equals(channel.Type, "category", StringComparison.OrdinalIgnoreCase));
        var syncedAt = await dbConnection.QuerySingleAsync<DateTime>(@"
          INSERT INTO discord.server_syncs
            (server_id, role_count, channel_count, category_count, synced_by_discord_id)
          VALUES
            (@ServerId, @RoleCount, @ChannelCount, @CategoryCount, @SyncedByDiscordId)
          RETURNING synced_at;
        ", new
        {
          ServerId = serverId,
          RoleCount = request.Roles.Count,
          ChannelCount = request.Channels.Count,
          CategoryCount = categoryCount,
          request.SyncedByDiscordId
        }, transaction);

        await transaction.CommitAsync();

        return Result<BotSyncResult, AppError>.Ok(new BotSyncResult
        {
          GuildId = request.GuildId,
          RoleCount = request.Roles.Count,
          ChannelCount = request.Channels.Count,
          CategoryCount = categoryCount,
          SyncedAt = syncedAt
        });
      }
      catch
      {
        await transaction.RollbackAsync();
        throw;
      }
    }
    catch (Exception ex)
    {
      return Result<BotSyncResult, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }


  private static BotGuildSummary MapGuildSummary(dynamic record) => new()
  {
    GuildId = (long)record.discord_guild_id,
    Name = (string)record.name,
    Enabled = (bool)record.enabled
  };

  private static BotGuildProfile MapGuildProfile(dynamic record, IReadOnlyCollection<BotSetupChannel> channels) => new()
  {
    GuildId = (long)record.discord_guild_id,
    Name = (string)record.name,
    Enabled = (bool)record.enabled,
    Theme = new BotTheme
    {
      PrimaryColor = (string)record.primary_color,
      ThumbnailUrl = record.thumbnail_url as string,
      FooterText = record.footer_text as string
    },
    Verification = new BotVerificationProfile
    {
      Enabled = (bool)record.verification_enabled,
      Title = (string)record.verification_title,
      Description = (string)record.verification_description,
      ButtonLabel = (string)record.verification_button_label,
      ChannelId = record.verification_channel_id as string,
      VerifiedRoleId = record.verified_role_id as string,
      BannerUrl = record.verification_banner_url as string
    },
    Welcome = new BotWelcomeProfile
    {
      Enabled = (bool)record.welcome_enabled,
      Title = (string)record.welcome_title,
      Description = (string)record.welcome_description,
      ChannelId = record.welcome_channel_id as string,
      BannerUrl = record.welcome_banner_url as string
    },
    SetupChannels = channels
  };

  private static BotCommandResponse MapCommandResponse(dynamic record) => new()
  {
    CommandName = (string)record.command_name,
    Title = (string)record.title,
    Description = (string)record.description,
    ImageUrl = record.image_url as string,
    Url = record.url as string,
    Color = record.color as string,
    Ephemeral = (bool)record.ephemeral
  };

  private static BotSetupChannel MapSetupChannel(dynamic record) => new()
  {
    Name = (string)record.name,
    Type = (string)record.type,
    Category = record.category as string,
    Position = (int)record.position
  };

  private static BotGuildProfile DefaultProfile(long guildId, IReadOnlyCollection<BotSetupChannel>? channels = null) => new()
  {
    GuildId = guildId,
    Name = $"Discord Server {guildId}",
    Enabled = true,
    SetupChannels = channels ?? DefaultSetupChannels()
  };

  private static BotCommandResponse DefaultCommandResponse(string commandName) => new()
  {
    CommandName = commandName,
    Title = $"/{commandName}",
    Description = $"/{commandName} is wired correctly. Configure this response in the backend for this server.",
    Ephemeral = true
  };

  private static IReadOnlyCollection<BotSetupChannel> DefaultSetupChannels() =>
  [
    new BotSetupChannel { Name = "welcome", Type = "text", Category = "Information", Position = 1 },
    new BotSetupChannel { Name = "verification", Type = "text", Category = "Information", Position = 2 },
    new BotSetupChannel { Name = "rules", Type = "text", Category = "Information", Position = 3 },
    new BotSetupChannel { Name = "general", Type = "text", Category = "Community", Position = 4 },
    new BotSetupChannel { Name = "bot-commands", Type = "text", Category = "Community", Position = 5 },
    new BotSetupChannel { Name = "General Voice", Type = "voice", Category = "Voice", Position = 6 }
  ];
}
