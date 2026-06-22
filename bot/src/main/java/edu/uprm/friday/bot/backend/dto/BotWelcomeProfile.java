package edu.uprm.friday.bot.backend.dto;

public record BotWelcomeProfile(
  boolean enabled,
  String title,
  String description,
  String channelId,
  String bannerUrl
) {
  public static BotWelcomeProfile defaults() {
    return new BotWelcomeProfile(
      true,
      "Bienvenido",
      "Gracias por unirte. Completa la verificacion para acceder al servidor.",
      null,
      null);
  }
}
