package edu.uprm.friday.bot.config;

import io.github.cdimascio.dotenv.Dotenv;

import java.util.Map;
import java.util.Optional;

public record BotConfig(
  String discordBotToken,
  Optional<String> discordClientId,
  Optional<Long> discordGuildId,
  String backendUrl
) {
  private static final String DEFAULT_BACKEND_URL = "http://localhost:8080";

  public static BotConfig load() {
    Dotenv dotenv = Dotenv.configure()
      .ignoreIfMissing()
      .load();

    return from(name -> {
      String envValue = System.getenv(name);
      if (envValue != null && !envValue.isBlank()) {
        return envValue;
      }
      return dotenv.get(name);
    });
  }

  public static BotConfig from(Map<String, String> values) {
    return from(values::get);
  }

  static BotConfig from(ConfigSource source) {
    String token = clean(source.get("DISCORD_BOT_TOKEN")).orElse("");
    Optional<String> clientId = clean(source.get("DISCORD_CLIENT_ID"));
    Optional<Long> guildId = clean(source.get("DISCORD_GUILD_ID")).flatMap(BotConfig::parseLong);
    String backendUrl = clean(firstPresent(source.get("BOT_BACKEND_URL"), source.get("BACKEND_URL")))
      .orElse(DEFAULT_BACKEND_URL);

    return new BotConfig(token, clientId, guildId, stripTrailingSlash(backendUrl));
  }

  public boolean hasUsableToken() {
    return !discordBotToken.isBlank() && !discordBotToken.startsWith("replace-with-");
  }

  private static String firstPresent(String preferred, String fallback) {
    return clean(preferred).orElse(fallback);
  }

  private static Optional<String> clean(String value) {
    if (value == null || value.isBlank()) {
      return Optional.empty();
    }
    return Optional.of(value.trim());
  }

  private static Optional<Long> parseLong(String value) {
    try {
      return Optional.of(Long.parseLong(value));
    } catch (NumberFormatException ignored) {
      return Optional.empty();
    }
  }

  private static String stripTrailingSlash(String value) {
    String trimmed = value.trim();
    while (trimmed.endsWith("/") && trimmed.length() > 1) {
      trimmed = trimmed.substring(0, trimmed.length() - 1);
    }
    return trimmed;
  }

  @FunctionalInterface
  interface ConfigSource {
    String get(String name);
  }
}
