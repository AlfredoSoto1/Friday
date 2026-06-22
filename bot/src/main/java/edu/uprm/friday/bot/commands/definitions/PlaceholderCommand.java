package edu.uprm.friday.bot.commands.definitions;

import edu.uprm.friday.bot.commands.BotCommand;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;
import net.dv8tion.jda.api.interactions.commands.build.Commands;

public final class PlaceholderCommand implements BotCommand {
    private final String name;
    private final CommandData commandData;

    public PlaceholderCommand(String name, String description) {
        this.name = name;
        this.commandData = Commands.slash(name, description);
    }

    @Override
    public CommandData commandData() {
        return commandData;
    }

    @Override
    public void handle(SlashCommandInteractionEvent event) {
        event.reply("/" + name + " is registered, but its implementation is not available yet.")
                .setEphemeral(true)
                .queue();
    }
}
