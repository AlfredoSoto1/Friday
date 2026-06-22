using Utils;

namespace Friday.Backend.Api.Domain;

public sealed class BotGuildQuery : BaseUrlQuery
{
}

public sealed class BotGuildSummary
{
  public long GuildId { get; init; }
  public string Name { get; init; } = string.Empty;
  public bool Enabled { get; init; }
}

public sealed class BotGuildProfile
{
  public long GuildId { get; init; }
  public string Name { get; init; } = string.Empty;
  public bool Enabled { get; init; } = true;
  public BotTheme Theme { get; init; } = new();
  public BotVerificationProfile Verification { get; init; } = new();
  public BotWelcomeProfile Welcome { get; init; } = new();
  public IReadOnlyCollection<BotSetupChannel> SetupChannels { get; init; } = [];
}

public sealed class BotTheme
{
  public string PrimaryColor { get; init; } = "2f80ed";
  public string? ThumbnailUrl { get; init; }
  public string? FooterText { get; init; }
}

public sealed class BotVerificationProfile
{
  public bool Enabled { get; init; } = true;
  public string Title { get; init; } = "Bienvenido al servidor";
  public string Description { get; init; } = "Presiona el boton de verificacion para confirmar tu correo institucional.";
  public string ButtonLabel { get; init; } = "Verify";
  public string? ChannelId { get; init; }
  public string? VerifiedRoleId { get; init; }
  public string? BannerUrl { get; init; }
}

public sealed class BotWelcomeProfile
{
  public bool Enabled { get; init; } = true;
  public string Title { get; init; } = "Bienvenido";
  public string Description { get; init; } = "Gracias por unirte. Completa la verificacion para acceder al servidor.";
  public string? ChannelId { get; init; }
  public string? BannerUrl { get; init; }
}

public sealed class BotCommandResponse
{
  public string CommandName { get; init; } = string.Empty;
  public string Title { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public string? ImageUrl { get; init; }
  public string? Url { get; init; }
  public string? Color { get; init; }
  public bool Ephemeral { get; init; } = true;
  public IReadOnlyCollection<BotButtonDefinition> Buttons { get; init; } = [];
}

public sealed class BotButtonDefinition
{
  public string Id { get; init; } = string.Empty;
  public string Label { get; init; } = string.Empty;
  public string Style { get; init; } = "primary";
  public string? Url { get; init; }
}

public sealed class BotSetupProfile
{
  public IReadOnlyCollection<BotSetupChannel> Channels { get; init; } = [];
}

public sealed class BotSetupChannel
{
  public string Name { get; init; } = string.Empty;
  public string Type { get; init; } = "text";
  public string? Category { get; init; }
  public int Position { get; init; }
}

public sealed class BotVerifyMemberRequest
{
  public string DiscordUserId { get; init; } = string.Empty;
  public string DiscordUsername { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string? FunFact { get; init; }
}

public sealed class BotVerifyMemberResult
{
  public bool Verified { get; init; }
  public string Message { get; init; } = string.Empty;
  public IReadOnlyCollection<string> RoleIds { get; init; } = [];
}

public sealed class BotXpRequest
{
  public string DiscordUserId { get; init; } = string.Empty;
  public string DiscordUsername { get; init; } = string.Empty;
  public int Amount { get; init; } = 1;
}

public sealed class BotXpResult
{
  public string DiscordUserId { get; init; } = string.Empty;
  public int Xp { get; init; }
  public int Level { get; init; }
  public bool LeveledUp { get; init; }
}
