package edu.uprm.friday.bot.commands;

import org.junit.jupiter.api.Test;

import java.util.Set;
import java.util.regex.Pattern;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

final class CommandRegistryTest {
    private static final Set<String> EXPECTED_COMMANDS = Set.of(
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
            "map"
    );

    @Test
    void registersEveryUniqueReadmeCommand() {
        CommandRegistry registry = CommandRegistry.createDefault();

        assertEquals(EXPECTED_COMMANDS.size(), registry.size());
        assertEquals(EXPECTED_COMMANDS, Set.copyOf(registry.names()));
    }

    @Test
    void commandNamesAreValidDiscordSlashNames() {
        Pattern slashCommandName = Pattern.compile("^[a-z0-9_-]{1,32}$");

        CommandRegistry registry = CommandRegistry.createDefault();

        registry.names().forEach(name -> assertTrue(
                slashCommandName.matcher(name).matches(),
                () -> "Invalid Discord slash command name: " + name
        ));
    }
}
