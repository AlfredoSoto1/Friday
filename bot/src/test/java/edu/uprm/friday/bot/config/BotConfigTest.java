package edu.uprm.friday.bot.config;

import org.junit.jupiter.api.Test;

import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

final class BotConfigTest {
  @Test
  void loadsUsableValues() {
    BotConfig config = BotConfig.from(Map.of(
      "DISCORD_BOT_TOKEN", "token",
      "DISCORD_CLIENT_ID", "123",
      "DISCORD_GUILD_ID", "456",
      "BOT_BACKEND_URL", "http://app:8080/"));

    assertTrue(config.hasUsableToken());
    assertEquals("token", config.discordBotToken());
    assertEquals("123", config.discordClientId().orElseThrow());
    assertEquals(456L, config.discordGuildId().orElseThrow());
    assertEquals("http://app:8080", config.backendUrl());
  }

  @Test
  void treatsPlaceholderTokenAsUnusable() {
    BotConfig config = BotConfig.from(Map.of("DISCORD_BOT_TOKEN", "replace-with-discord-bot-token"));

    assertFalse(config.hasUsableToken());
  }

  @Test
  void ignoresInvalidGuildIdAndUsesBackendUrlFallback() {
    BotConfig config = BotConfig.from(Map.of(
      "DISCORD_GUILD_ID", "not-a-number",
      "BACKEND_URL", "http://backend:8080"));

    assertTrue(config.discordGuildId().isEmpty());
    assertEquals("http://backend:8080", config.backendUrl());
  }
}
