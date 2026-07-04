package assistant.app.interactions;

import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;

public interface SlashCommandDefinition {
  CommandData commandData();

  void handle(SlashCommandInteractionEvent event);
}
