package edu.uprm.friday.bot.commands;

import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;

public interface BotCommand {
    /**
     * Gets the command data for this command, which includes its name, description,
     * and options.
     * 
     * @return the command data for this command
     */
    CommandData commandData();

    /**
     * Handles the execution of this command when it is invoked by a user. This
     * method
     * is called when the command is triggered, and it should contain the logic for
     * processing the command and responding to the user.
     * 
     * @param event
     */
    void handle(SlashCommandInteractionEvent event);
}
