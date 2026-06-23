namespace Friday.Backend.Api.Domain;

public sealed class BotSyncRequest
{
  public long GuildId { get; init; }
  public string GuildName { get; init; } = string.Empty;
  public string SyncedByDiscordId { get; init; } = string.Empty;
  public IReadOnlyCollection<BotSyncRole> Roles { get; init; } = [];
  public IReadOnlyCollection<BotSyncChannel> Channels { get; init; } = [];
}

public sealed class BotSyncResult
{
  public long GuildId { get; init; }
  public int RoleCount { get; init; }
  public int ChannelCount { get; init; }
  public int CategoryCount { get; init; }
  public DateTime SyncedAt { get; init; }
}
