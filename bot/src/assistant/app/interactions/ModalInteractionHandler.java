package assistant.app.interactions;

import net.dv8tion.jda.api.events.interaction.ModalInteractionEvent;

@FunctionalInterface
public interface ModalInteractionHandler {
  void handle(ModalInteractionEvent event);
}
