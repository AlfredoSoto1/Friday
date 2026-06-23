package edu.uprm.friday.bot.backend.dto;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

final class BotDefaultsTest {
  @Test
  void defaultGuildProfileIsEnabledAndHasSetupChannels() {
    BotGuildProfile profile = BotGuildProfile.defaultFor(123L);

    assertEquals(123L, profile.guildId());
    assertTrue(profile.enabled());
    assertTrue(profile.verification().enabled());
  }

  @Test
  void defaultCommandResponseUsesRequestedCommandName() {
    BotCommandResponse response = BotCommandResponse.defaultFor("rules");

    assertEquals("rules", response.commandName());
    assertEquals("/rules", response.title());
    assertTrue(response.ephemeral());
  }
}
