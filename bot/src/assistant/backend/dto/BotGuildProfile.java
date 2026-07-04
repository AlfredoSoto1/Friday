package assistant.backend.dto;

public record BotGuildProfile(
  long guildId,
  String name,
  boolean enabled,
  String departmentProfile,
  String createdAt,
  BotTheme theme,
  BotVerificationProfile verification,
  BotWelcomeProfile welcome
) {
  public static BotGuildProfile defaultFor(long guildId) {
    return new BotGuildProfile(
      guildId,
      "Discord Server " + guildId,
      true,
      null,
      null,
      BotTheme.defaults(),
      BotVerificationProfile.defaults(),
      BotWelcomeProfile.defaults());
  }
}
