package edu.uprm.friday.bot.backend.dto;

import java.util.List;

public record BotSyncRequest(
  long guildId,
  String guildName,
  String syncedByDiscordId,
  List<BotSyncRole> roles,
  List<BotSyncChannel> channels
) {
}
