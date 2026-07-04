package assistant.app.interactions;

import net.dv8tion.jda.api.events.interaction.component.StringSelectInteractionEvent;

@FunctionalInterface
public interface SelectMenuInteractionHandler {
  void handle(StringSelectInteractionEvent event);
}
