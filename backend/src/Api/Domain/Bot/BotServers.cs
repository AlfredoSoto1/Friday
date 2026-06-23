namespace Friday.Backend.Api.Domain;

public sealed class BotGuildSummary
{
  public long GuildId { get; init; }
  public string Name { get; init; } = string.Empty;
  public bool Enabled { get; init; } = true;
}

public sealed class BotGuildProfile
{
  public long GuildId { get; init; }
  public string Name { get; init; } = string.Empty;
  public bool Enabled { get; init; } = true;
  public BotTheme Theme { get; init; } = new();
  public BotVerificationProfile Verification { get; init; } = new();
  public BotWelcomeProfile Welcome { get; init; } = new();
}

public sealed class BotTheme
{
  public string PrimaryColor { get; init; } = "2f80ed";
  public string? ThumbnailUrl { get; init; }
  public string? FooterText { get; init; } = "Friday";
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
