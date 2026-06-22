package edu.uprm.friday.bot.config;

import org.junit.jupiter.api.Test;

import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

final class BotConfigTest {
  @Test
  void loadsTokenAndBackendUrl() {
    BotConfig config = BotConfig.from(Map.of(
      "DISCORD_BOT_TOKEN", "token",
      "BACKEND_URL", "http://app:8080/"));

    assertTrue(config.hasUsableToken());
    assertEquals("token", config.discordBotToken());
    assertEquals("http://app:8080", config.backendUrl());
  }

  @Test
  void treatsPlaceholderTokenAsUnusable() {
    BotConfig config = BotConfig.from(Map.of("DISCORD_BOT_TOKEN", "replace-with-discord-bot-token"));

    assertFalse(config.hasUsableToken());
  }

  @Test
  void usesLocalBackendUrlWhenBackendUrlIsMissing() {
    BotConfig config = BotConfig.from(Map.of("DISCORD_BOT_TOKEN", "token"));

    assertEquals("http://localhost:8080", config.backendUrl());
  }
}
