package assistant.backend.dto;

public record BotSyncChannel(
  String discordChannelId,
  String parentChannelId,
  String name,
  String type,
  int position,
  String topic,
  boolean nsfw
) {
}
