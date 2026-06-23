namespace Friday.Backend.Api.Domain;

public sealed class BotUser
{
  public int UserId { get; init; }
  public string Email { get; init; } = string.Empty;
  public string Fullname { get; init; } = string.Empty;
  public string Username { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public class BotUserRequest
{
  public string Email { get; init; } = string.Empty;
  public string Fullname { get; init; } = string.Empty;
  public string Username { get; init; } = string.Empty;
}

public sealed class RegisterGuildMemberRequest : BotUserRequest
{
  public string DiscordUserId { get; init; } = "-";
  public string? FunFact { get; init; }
  public IReadOnlyCollection<string> DiscordRoleIds { get; init; } = [];
}

public sealed class BulkRegisterGuildMembersRequest
{
  public IReadOnlyCollection<RegisterGuildMemberRequest> Members { get; init; } = [];
}

public sealed class BotServerMember
{
  public int ServerUserId { get; init; }
  public int UserId { get; init; }
  public string Email { get; init; } = string.Empty;
  public string Fullname { get; init; } = string.Empty;
  public string Username { get; init; } = string.Empty;
  public string DiscordUserId { get; init; } = string.Empty;
  public bool Verified { get; init; }
  public string? FunFact { get; init; }
  public int Xp { get; init; }
  public int Level { get; init; }
  public IReadOnlyCollection<string> RoleIds { get; init; } = [];
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
}

public sealed class BotRole
{
  public int RoleId { get; init; }
  public long GuildId { get; init; }
  public string? DiscordRoleId { get; init; }
  public string Name { get; init; } = string.Empty;
  public int? Color { get; init; }
  public int? Position { get; init; }
  public bool Managed { get; init; }
  public bool Mentionable { get; init; }
  public bool Hoisted { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
}

public sealed class BotRoleRequest
{
  public string? DiscordRoleId { get; init; }
  public string Name { get; init; } = string.Empty;
  public int? Color { get; init; }
  public int? Position { get; init; }
  public bool Managed { get; init; }
  public bool Mentionable { get; init; }
  public bool Hoisted { get; init; }
}

public sealed class BotChannel
{
  public int ChannelId { get; init; }
  public long GuildId { get; init; }
  public string DiscordChannelId { get; init; } = string.Empty;
  public string? ParentChannelId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Type { get; init; } = string.Empty;
  public int? Position { get; init; }
  public string? Topic { get; init; }
  public bool Nsfw { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
}

public sealed class BotChannelRequest
{
  public string DiscordChannelId { get; init; } = string.Empty;
  public string? ParentChannelId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Type { get; init; } = "text";
  public int? Position { get; init; }
  public string? Topic { get; init; }
  public bool Nsfw { get; init; }
}

public sealed class GuildProfileRequest
{
  public string Name { get; init; } = string.Empty;
  public bool Enabled { get; init; } = true;
  public Theme Theme { get; init; } = new();
  public VerificationProfile Verification { get; init; } = new();
  public WelcomeProfile Welcome { get; init; } = new();
}
