package assistant.backend.dto;

public record BotSyncRole(
  String discordRoleId,
  String name,
  int color,
  int position,
  boolean managed,
  boolean mentionable,
  boolean hoisted
) {
}
