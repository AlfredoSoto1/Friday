namespace Friday.Backend.Api.Domain;

public sealed class BotSyncRequest
{
  public long GuildId { get; init; }
  public string GuildName { get; init; } = string.Empty;
  public IReadOnlyCollection<BotSyncRole> Roles { get; init; } = [];
}

public sealed class BotSyncResult
{
  public long GuildId { get; init; }
  public int RoleCount { get; init; }
  public bool ServerRegistered { get; init; }
  public DateTime? SyncedAt { get; init; }
}
