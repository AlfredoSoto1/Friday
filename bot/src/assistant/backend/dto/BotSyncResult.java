package assistant.backend.dto;

public record BotSyncResult(
    long guildId, int roleCount, boolean serverRegistered, String syncedAt) {
  public static BotSyncResult unavailable(long guildId) {
    return new BotSyncResult(guildId, 0, true, null);
  }
}
