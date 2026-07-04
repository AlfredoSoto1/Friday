package assistant.app.interactions;

import net.dv8tion.jda.api.events.interaction.component.ButtonInteractionEvent;

@FunctionalInterface
public interface ButtonInteractionHandler {
  void handle(ButtonInteractionEvent event);
}
