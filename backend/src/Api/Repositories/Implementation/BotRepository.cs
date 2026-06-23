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
        SELECT server_code, name
          FROM discord.servers
        ORDER BY name;
      ";

      var records = await connection.QueryAsync(sql);
      var guilds = records.Select(record => new BotGuildSummary
      {
        GuildId = long.Parse((string)record.server_code),
        Name = (string)record.name,
        Enabled = true
      }).ToArray();

      return Result<IReadOnlyCollection<BotGuildSummary>, AppError>.Ok(guilds);
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
      const string sql = @"
        SELECT name
          FROM discord.servers
        WHERE server_code = @GuildId;
      ";

      var name = await connection.QueryFirstOrDefaultAsync<string>(sql, new { GuildId = guildId.ToString() });
      return Result<BotGuildProfile, AppError>.Ok(new BotGuildProfile
      {
        GuildId = guildId,
        Name = name ?? $"Discord Server {guildId}",
        Enabled = true
      });
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
      var response = commandName switch
      {
        "faculty" => await QueryFacultyResponse(connection),
        "ls_projects" => await QueryProjectsResponse(connection),
        "ls_student_orgs" => await QueryOrganizationsResponse(connection),
        "salon" => await QueryRoomsResponse(connection, "salon"),
        "lab" => await QueryRoomsResponse(connection, "lab"),
        "contact-department" => await QueryDepartmentContactsResponse(connection),
        "contact-dcsp" => await QueryNamedContactResponse(connection, commandName, "DCSP"),
        "contact-decanato-estudiantes" => await QueryNamedContactResponse(connection, commandName, "Decanato"),
        "contact-guardia-univ" => await QueryNamedContactResponse(connection, commandName, "Guardia"),
        "contact-asesoria-academica" => await QueryNamedContactResponse(connection, commandName, "Asesoria"),
        "contact-asistencia-economica" => await QueryNamedContactResponse(connection, commandName, "Asistencia"),
        _ => DefaultCommandResponse(commandName)
      };

      return Result<BotCommandResponse, AppError>.Ok(response);
    }
    catch (Exception ex)
    {
      return Result<BotCommandResponse, AppError>.Fail(AppError.BadRequest(ex.Message));
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
        WITH target_server AS (
          INSERT INTO discord.servers (name, server_code)
          VALUES (@ServerName, @GuildId)
          ON CONFLICT (server_code)
          DO UPDATE SET name = discord.servers.name
          RETURNING server_id
        ), target_user AS (
          INSERT INTO discord.users (email, fullname, username)
          VALUES (@Email, @Username, @Username)
          ON CONFLICT (email)
          DO UPDATE SET username = EXCLUDED.username
          RETURNING user_id
        )
        INSERT INTO discord.servers_users (server_id, user_id, verified)
        SELECT target_server.server_id, target_user.user_id, TRUE
          FROM target_server, target_user
        ON CONFLICT DO NOTHING
        RETURNING su_id;
      ";

      await connection.ExecuteAsync(sql, new
      {
        ServerName = $"Discord Server {guildId}",
        GuildId = guildId.ToString(),
        request.Email,
        Username = request.DiscordUsername
      });

      return Result<BotVerifyMemberResult, AppError>.Ok(new BotVerifyMemberResult
      {
        Verified = true,
        Message = "Verification completed.",
        RoleIds = []
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
        WITH target_server AS (
          INSERT INTO discord.servers (name, server_code)
          VALUES (@ServerName, @GuildId)
          ON CONFLICT (server_code)
          DO UPDATE SET name = discord.servers.name
          RETURNING server_id
        ), target_user AS (
          INSERT INTO discord.users (email, fullname, username)
          VALUES (@SyntheticEmail, @Username, @Username)
          ON CONFLICT (email)
          DO UPDATE SET username = EXCLUDED.username
          RETURNING user_id
        ), target_server_user AS (
          INSERT INTO discord.servers_users (server_id, user_id, verified)
          SELECT target_server.server_id, target_user.user_id, FALSE
            FROM target_server, target_user
          ON CONFLICT DO NOTHING
          RETURNING su_id
        ), selected_server_user AS (
          SELECT su_id FROM target_server_user
          UNION ALL
          SELECT servers_users.su_id
            FROM discord.servers_users
            INNER JOIN target_server ON target_server.server_id = servers_users.server_id
            INNER JOIN target_user ON target_user.user_id = servers_users.user_id
          LIMIT 1
        )
        INSERT INTO discord.user_levels (su_id, xp, level)
        SELECT su_id, @Amount, 1
          FROM selected_server_user
        ON CONFLICT (su_id)
        DO UPDATE SET xp = discord.user_levels.xp + EXCLUDED.xp,
                      level = GREATEST(1, FLOOR(SQRT((discord.user_levels.xp + EXCLUDED.xp) / 25.0))::INT + 1),
                      updated_at = CURRENT_TIMESTAMP
        RETURNING xp, level;
      ";

      var record = await connection.QuerySingleAsync(sql, new
      {
        ServerName = $"Discord Server {guildId}",
        GuildId = guildId.ToString(),
        SyntheticEmail = $"discord-{request.DiscordUserId}@users.local",
        Username = request.DiscordUsername,
        request.Amount
      });

      return Result<BotXpResult, AppError>.Ok(new BotXpResult
      {
        DiscordUserId = request.DiscordUserId,
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
      var serverId = await UpsertServer(dbConnection, transaction, request);
      await UpsertRoles(dbConnection, transaction, serverId, request.Roles);
      await UpsertChannels(dbConnection, transaction, serverId, request.Channels);
      var syncedAt = await InsertSyncAudit(dbConnection, transaction, serverId, request);
      await transaction.CommitAsync();

      return Result<BotSyncResult, AppError>.Ok(new BotSyncResult
      {
        GuildId = request.GuildId,
        RoleCount = request.Roles.Count,
        ChannelCount = request.Channels.Count,
        CategoryCount = request.Channels.Count(channel => string.Equals(channel.Type, "category", StringComparison.OrdinalIgnoreCase)),
        SyncedAt = syncedAt
      });
    }
    catch (Exception ex)
    {
      return Result<BotSyncResult, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static async Task<int> UpsertServer(
    System.Data.Common.DbConnection connection,
    System.Data.Common.DbTransaction transaction,
    BotSyncRequest request)
  {
    const string sql = @"
      INSERT INTO discord.servers (name, server_code)
      VALUES (@Name, @ServerCode)
      ON CONFLICT (server_code)
      DO UPDATE SET name = EXCLUDED.name
      RETURNING server_id;
    ";

    return await connection.QuerySingleAsync<int>(sql, new
    {
      Name = request.GuildName,
      ServerCode = request.GuildId.ToString()
    }, transaction);
  }

  private static async Task UpsertRoles(
    System.Data.Common.DbConnection connection,
    System.Data.Common.DbTransaction transaction,
    int serverId,
    IReadOnlyCollection<BotSyncRole> roles)
  {
    const string sql = @"
      INSERT INTO discord.roles
        (server_id, discord_role_id, name, color, position, managed, mentionable, hoisted, updated_at)
      SELECT @ServerId, discord_role_id, name, color, position, managed, mentionable, hoisted, CURRENT_TIMESTAMP
        FROM UNNEST(@DiscordRoleIds, @Names, @Colors, @Positions, @Managed, @Mentionable, @Hoisted)
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

    await connection.ExecuteAsync(sql, new
    {
      ServerId = serverId,
      DiscordRoleIds = roles.Select(role => role.DiscordRoleId).ToArray(),
      Names = roles.Select(role => role.Name).ToArray(),
      Colors = roles.Select(role => role.Color).ToArray(),
      Positions = roles.Select(role => role.Position).ToArray(),
      Managed = roles.Select(role => role.Managed).ToArray(),
      Mentionable = roles.Select(role => role.Mentionable).ToArray(),
      Hoisted = roles.Select(role => role.Hoisted).ToArray()
    }, transaction);
  }

  private static async Task UpsertChannels(
    System.Data.Common.DbConnection connection,
    System.Data.Common.DbTransaction transaction,
    int serverId,
    IReadOnlyCollection<BotSyncChannel> channels)
  {
    const string sql = @"
      INSERT INTO discord.channels
        (server_id, discord_channel_id, parent_channel_id, name, type, position, topic, nsfw, updated_at)
      SELECT @ServerId, discord_channel_id, parent_channel_id, name, type, position, topic, nsfw, CURRENT_TIMESTAMP
        FROM UNNEST(@DiscordChannelIds, @ParentChannelIds, @Names, @Types, @Positions, @Topics, @Nsfw)
          AS synced(discord_channel_id, parent_channel_id, name, type, position, topic, nsfw)
      ON CONFLICT (server_id, discord_channel_id)
      DO UPDATE SET parent_channel_id = EXCLUDED.parent_channel_id,
                    name = EXCLUDED.name,
                    type = EXCLUDED.type,
                    position = EXCLUDED.position,
                    topic = EXCLUDED.topic,
                    nsfw = EXCLUDED.nsfw,
                    updated_at = CURRENT_TIMESTAMP;
    ";

    await connection.ExecuteAsync(sql, new
    {
      ServerId = serverId,
      DiscordChannelIds = channels.Select(channel => channel.DiscordChannelId).ToArray(),
      ParentChannelIds = channels.Select(channel => channel.ParentChannelId).ToArray(),
      Names = channels.Select(channel => channel.Name).ToArray(),
      Types = channels.Select(channel => channel.Type).ToArray(),
      Positions = channels.Select(channel => channel.Position).ToArray(),
      Topics = channels.Select(channel => channel.Topic).ToArray(),
      Nsfw = channels.Select(channel => channel.Nsfw).ToArray()
    }, transaction);
  }

  private static async Task<DateTime> InsertSyncAudit(
    System.Data.Common.DbConnection connection,
    System.Data.Common.DbTransaction transaction,
    int serverId,
    BotSyncRequest request)
  {
    const string sql = @"
      INSERT INTO discord.server_syncs
        (server_id, role_count, channel_count, category_count, synced_by_discord_id)
      VALUES
        (@ServerId, @RoleCount, @ChannelCount, @CategoryCount, @SyncedByDiscordId)
      RETURNING synced_at;
    ";

    return await connection.QuerySingleAsync<DateTime>(sql, new
    {
      ServerId = serverId,
      RoleCount = request.Roles.Count,
      ChannelCount = request.Channels.Count,
      CategoryCount = request.Channels.Count(channel => string.Equals(channel.Type, "category", StringComparison.OrdinalIgnoreCase)),
      request.SyncedByDiscordId
    }, transaction);
  }

  private static async Task<BotCommandResponse> QueryFacultyResponse(IDbConnection connection)
  {
    const string sql = @"
      SELECT COALESCE(string_agg(format('• %s', name), E'\n' ORDER BY name), 'No faculty records found.') AS description
        FROM inelicom.faculties;
    ";

    var description = await connection.QuerySingleAsync<string>(sql);
    return EmbedResponse("faculty", "Faculty", description);
  }

  private static async Task<BotCommandResponse> QueryProjectsResponse(IDbConnection connection)
  {
    const string sql = @"
      SELECT COALESCE(string_agg(format('• %s — %s', name, description), E'\n' ORDER BY name), 'No project records found.') AS description
        FROM inelicom.projects;
    ";

    var description = await connection.QuerySingleAsync<string>(sql);
    return EmbedResponse("ls_projects", "Projects and Research", description);
  }

  private static async Task<BotCommandResponse> QueryOrganizationsResponse(IDbConnection connection)
  {
    const string sql = @"
      SELECT COALESCE(string_agg(format('• %s — %s', name, description), E'\n' ORDER BY name), 'No student organization records found.') AS description
        FROM inelicom.organizations;
    ";

    var description = await connection.QuerySingleAsync<string>(sql);
    return EmbedResponse("ls_student_orgs", "Student Organizations", description);
  }

  private static async Task<BotCommandResponse> QueryRoomsResponse(IDbConnection connection, string commandName)
  {
    const string sql = @"
      SELECT COALESCE(string_agg(format('• %s — %s', code, name), E'\n' ORDER BY code), 'No room records found.') AS description
        FROM inelicom.rooms;
    ";

    var description = await connection.QuerySingleAsync<string>(sql);
    return EmbedResponse(commandName, commandName == "lab" ? "Labs" : "Rooms", description);
  }

  private static async Task<BotCommandResponse> QueryDepartmentContactsResponse(IDbConnection connection)
  {
    const string sql = @"
      SELECT COALESCE(string_agg(format('• %s', name), E'\n' ORDER BY name), 'No department records found.') AS description
        FROM inelicom.departments;
    ";

    var description = await connection.QuerySingleAsync<string>(sql);
    return EmbedResponse("contact-department", "Departments", description);
  }

  private static async Task<BotCommandResponse> QueryNamedContactResponse(IDbConnection connection, string commandName, string search)
  {
    const string sql = @"
      SELECT COALESCE(string_agg(format('• %s — %s — %s — %s', name, email, phone, website), E'\n' ORDER BY name), 'No matching contact records found.') AS description
        FROM inelicom.contacts
      WHERE name ILIKE @Search;
    ";

    var description = await connection.QuerySingleAsync<string>(sql, new { Search = $"%{search}%" });
    return EmbedResponse(commandName, "Contact Information", description);
  }

  private static BotCommandResponse EmbedResponse(string commandName, string title, string description) => new()
  {
    CommandName = commandName,
    Title = title,
    Description = description,
    Ephemeral = true
  };

  private static BotCommandResponse DefaultCommandResponse(string commandName) => new()
  {
    CommandName = commandName,
    Title = $"/{commandName}",
    Description = $"/{commandName} is wired. Add backend data for a richer response.",
    Ephemeral = true
  };
}
