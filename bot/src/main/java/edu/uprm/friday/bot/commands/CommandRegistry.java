package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.commands.definitions.PlaceholderCommand;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;

import java.util.Collection;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;

public final class CommandRegistry {
    private final Map<String, BotCommand> commandsByName;

    private CommandRegistry(Collection<BotCommand> commands) {
        Map<String, BotCommand> commandsByName = new LinkedHashMap<>();
        for (BotCommand command : commands) {
            String name = command.commandData().getName();
            BotCommand previous = commandsByName.putIfAbsent(name, command);
            if (previous != null) {
                throw new IllegalArgumentException("Duplicate Discord command registered: " + name);
            }
        }
        this.commandsByName = Map.copyOf(commandsByName);
    }

    public static CommandRegistry createDefault() {
        return new CommandRegistry(List.of(
                placeholder("calendario", "Links to the academic calendar of UPRM."),
                placeholder("estudiante-orientador", "Lists all EO's in the server by department."),
                placeholder("rules", "Displays the rules and guidelines for the Discord server."),
                placeholder("guia-prepistica", "Provides a guide for incoming freshmen."),
                placeholder("faculty", "Displays detailed information about faculty members."),
                placeholder("ls_projects", "Provides information on ongoing projects and research."),
                placeholder("ls_student_orgs", "Lists student organizations and their activities."),
                placeholder("salon", "Finds a building on campus based on its code."),
                placeholder("lab", "Finds a lab on campus based on its code."),
                placeholder("links", "Provides useful links related to studies and resources."),
                placeholder("made-web", "Links to the MADE website."),
                placeholder("contact-dcsp",
                        "Provides information about the Department of Computer Science and Engineering."),
                placeholder("contact-department", "Displays information about specific departments at UPRM."),
                placeholder("contact-decanato-estudiantes", "Provides contact information for the Dean of Students."),
                placeholder("contact-guardia-univ", "Provides contact information for campus security."),
                placeholder("contact-asesoria-academica", "Provides guidance on academic matters and advisories."),
                placeholder("contact-asistencia-economica",
                        "Provides information about financial aid and economic assistance."),
                placeholder("curriculo", "Provides general information about academic curricula."),
                placeholder("faq", "Answers frequently asked questions about the Discord server."),
                placeholder("help", "Displays a comprehensive help menu for navigating the bot's commands."),
                placeholder("map", "Provides a link to the UPRM campus map.")));
    }

    public List<CommandData> commandData() {
        return commandsByName.values().stream()
                .map(BotCommand::commandData)
                .toList();
    }

    public Optional<BotCommand> find(String name) {
        return Optional.ofNullable(commandsByName.get(name));
    }

    public void handle(SlashCommandInteractionEvent event) {
        find(event.getName()).ifPresentOrElse(
                command -> command.handle(event),
                () -> event.reply("Unknown command: /" + event.getName()).setEphemeral(true).queue());
    }

    public int size() {
        return commandsByName.size();
    }

    public Collection<String> names() {
        return commandsByName.keySet();
    }

    private static BotCommand placeholder(String name, String description) {
        return new PlaceholderCommand(name, description);
    }
}
