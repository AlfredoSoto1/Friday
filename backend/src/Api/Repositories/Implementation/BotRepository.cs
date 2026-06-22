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
