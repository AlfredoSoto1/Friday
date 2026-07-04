package edu.uprm.friday.bot.interactions;

import edu.uprm.friday.bot.commands.CommandCatalog;
import edu.uprm.friday.bot.embeds.EmbedFactory;
import org.junit.jupiter.api.Test;

import java.util.Set;
import java.util.regex.Pattern;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

final class InteractionRegistryTest {
  private static final Set<String> README_COMMANDS = Set.of(
    "calendario",
    "estudiante-orientador",
    "rules",
    "guia-prepistica",
    "faculty",
    "ls_projects",
    "ls_student_orgs",
    "salon",
    "lab",
    "links",
    "made-web",
    "contact-dcsp",
    "contact-department",
    "contact-decanato-estudiantes",
    "contact-guardia-univ",
    "contact-asesoria-academica",
    "contact-asistencia-economica",
    "curriculo",
    "faq",
    "help",
    "map");

  @Test
  void registersReadmeCommandsAndCoreFeatureCommands() {
    InteractionRegistry registry = CommandCatalog.createDefault(new EmbedFactory());

    assertTrue(registry.commandNames().containsAll(README_COMMANDS));
    assertTrue(registry.commandNames().contains("verification-panel"));
    assertTrue(registry.commandNames().contains("sync"));
    assertTrue(registry.commandNames().contains("coinflip"));
    assertEquals(README_COMMANDS.size() + 3, registry.commandCount());
  }

  @Test
  void commandNamesAreValidDiscordSlashNames() {
    Pattern slashCommandName = Pattern.compile("^[a-z0-9_-]{1,32}$");
    InteractionRegistry registry = CommandCatalog.createDefault(new EmbedFactory());

    registry.commandNames().forEach(name -> assertTrue(
      slashCommandName.matcher(name).matches(),
      () -> "Invalid Discord slash command name: " + name));
  }

  @Test
  void registersCustomVerificationInteractions() {
    InteractionRegistry registry = CommandCatalog.createDefault(new EmbedFactory());

    assertTrue(registry.button("friday:verification:start").isPresent());
    assertTrue(registry.modal("friday:verification:modal").isPresent());
  }
}
