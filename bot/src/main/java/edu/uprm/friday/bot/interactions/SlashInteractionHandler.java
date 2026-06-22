package edu.uprm.friday.bot.interactions;

import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;

@FunctionalInterface
public interface SlashInteractionHandler {
  void handle(SlashCommandInteractionEvent event);
}
