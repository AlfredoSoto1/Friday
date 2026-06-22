package edu.uprm.friday.bot.backend.dto;

import java.util.List;

public record BotSetupProfile(List<BotSetupChannel> channels) {
  public static BotSetupProfile defaultProfile() {
    return new BotSetupProfile(defaultChannels());
  }

  public static List<BotSetupChannel> defaultChannels() {
    return List.of(
      new BotSetupChannel("welcome", "text", "Information", 1),
      new BotSetupChannel("verification", "text", "Information", 2),
      new BotSetupChannel("rules", "text", "Information", 3),
      new BotSetupChannel("general", "text", "Community", 4),
      new BotSetupChannel("bot-commands", "text", "Community", 5),
      new BotSetupChannel("General Voice", "voice", "Voice", 6));
  }
}
