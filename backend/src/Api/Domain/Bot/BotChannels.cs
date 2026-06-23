namespace Friday.Backend.Api.Domain;

public sealed class BotSyncChannel
{
  public string DiscordChannelId { get; init; } = string.Empty;
  public string? ParentChannelId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Type { get; init; } = string.Empty;
  public int Position { get; init; }
  public string? Topic { get; init; }
  public bool Nsfw { get; init; }
}
