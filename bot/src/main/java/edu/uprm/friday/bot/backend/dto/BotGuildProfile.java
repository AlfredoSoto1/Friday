package edu.uprm.friday.bot.backend.dto;

import java.util.List;

public record BotGuildProfile(
  long guildId,
  String name,
  boolean enabled,
  BotTheme theme,
  BotVerificationProfile verification,
  BotWelcomeProfile welcome,
  List<BotSetupChannel> setupChannels
) {
  public static BotGuildProfile defaultFor(long guildId) {
    return new BotGuildProfile(
      guildId,
      "Discord Server " + guildId,
      true,
      BotTheme.defaults(),
      BotVerificationProfile.defaults(),
      BotWelcomeProfile.defaults(),
      BotSetupProfile.defaultChannels());
  }
}
