namespace Friday.Backend.Api.Domain;

public sealed class BotSyncRole
{
  public string DiscordRoleId { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public int Color { get; init; }
  public int Position { get; init; }
  public bool Managed { get; init; }
  public bool Mentionable { get; init; }
  public bool Hoisted { get; init; }
}
