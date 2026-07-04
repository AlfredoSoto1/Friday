package assistant.app.interactions;

import net.dv8tion.jda.api.interactions.commands.build.CommandData;

import java.util.Collection;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;

public final class InteractionRegistry {
  private final List<InteractionDefinition> interactions;
  private final Map<String, SlashCommandDefinition> commandsByName;
  private final Map<String, ButtonInteractionHandler> buttonsById;
  private final Map<String, ModalInteractionHandler> modalsById;
  private final Map<String, SelectMenuInteractionHandler> selectMenusById;

  public InteractionRegistry(Collection<InteractionDefinition> interactions) {
    this.interactions = List.copyOf(interactions);
    this.commandsByName = new LinkedHashMap<>();
    this.buttonsById = new LinkedHashMap<>();
    this.modalsById = new LinkedHashMap<>();
    this.selectMenusById = new LinkedHashMap<>();

    for (InteractionDefinition interaction : interactions) {
      if (interaction instanceof SlashCommandDefinition command) {
        String name = command.commandData().getName();
        if (commandsByName.putIfAbsent(name, command) != null) {
          throw new IllegalArgumentException("Duplicate slash command registered: " + name);
        }
      }
      buttonsById.putAll(interaction.buttons());
      modalsById.putAll(interaction.modals());
      selectMenusById.putAll(interaction.selectMenus());
    }
  }

  public List<InteractionDefinition> interactions() {
    return interactions;
  }

  public List<CommandData> commandData() {
    return commandsByName.values().stream()
      .map(SlashCommandDefinition::commandData)
      .toList();
  }

  public Optional<SlashCommandDefinition> command(String name) {
    return Optional.ofNullable(commandsByName.get(name));
  }

  public Optional<ButtonInteractionHandler> button(String componentId) {
    return Optional.ofNullable(buttonsById.get(componentId));
  }

  public Optional<ModalInteractionHandler> modal(String modalId) {
    return Optional.ofNullable(modalsById.get(modalId));
  }

  public Optional<SelectMenuInteractionHandler> selectMenu(String componentId) {
    return Optional.ofNullable(selectMenusById.get(componentId));
  }

  public int commandCount() {
    return commandsByName.size();
  }

  public Collection<String> commandNames() {
    return commandsByName.keySet();
  }
}
