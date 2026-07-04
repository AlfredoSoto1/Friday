package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.backend.dto.BotButtonDefinition;
import edu.uprm.friday.bot.backend.dto.BotCommandResponse;
import edu.uprm.friday.bot.backend.dto.BotGuildProfile;
import edu.uprm.friday.bot.embeds.EmbedFactory;
import edu.uprm.friday.bot.interactions.InteractionDefinition;
import edu.uprm.friday.bot.interactions.SlashCommandDefinition;
import edu.uprm.friday.bot.interactions.SlashInteractionHandler;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;
import net.dv8tion.jda.api.interactions.commands.build.Commands;
import net.dv8tion.jda.api.interactions.components.buttons.Button;
import net.dv8tion.jda.api.requests.restaction.interactions.ReplyCallbackAction;

import java.util.List;

public class BackendContentCommand extends InteractionDefinition implements SlashCommandDefinition {
  private final String name;
  private final CommandData commandData;
  private final BackendClient backendClient;
  private final EmbedFactory embedFactory;
  private SlashInteractionHandler customHandler;

  public BackendContentCommand(String name, String description, BackendClient backendClient, EmbedFactory embedFactory) {
    this.name = name;
    this.commandData = Commands.slash(name, description);
    this.backendClient = backendClient;
    this.embedFactory = embedFactory;
  }

  public BackendContentCommand withCustomHandler(SlashInteractionHandler handler) {
    this.customHandler = handler;
    return this;
  }

  @Override
  public CommandData commandData() {
    return commandData;
  }

  @Override
  public void handle(SlashCommandInteractionEvent event) {
    if (customHandler != null) {
      customHandler.handle(event);
      return;
    }

    long guildId = event.getGuild() == null ? 0 : event.getGuild().getIdLong();
    BotGuildProfile profile = backendClient.guildProfile(guildId);
    BotCommandResponse response = backendClient.commandResponse(guildId, name);

    ReplyCallbackAction reply = event.replyEmbeds(embedFactory.commandEmbed(profile, response))
      .setEphemeral(response.ephemeral());

    List<Button> buttons = toButtons(response.buttons());
    if (!buttons.isEmpty()) {
      reply = reply.setActionRow(buttons);
    }

    reply.queue();
  }

  private static List<Button> toButtons(List<BotButtonDefinition> definitions) {
    return definitions.stream()
      .limit(5)
      .map(BackendContentCommand::toButton)
      .toList();
  }

  private static Button toButton(BotButtonDefinition definition) {
    if (definition.url() != null && !definition.url().isBlank()) {
      return Button.link(definition.url(), definition.label());
    }

    String style = definition.style() == null ? "" : definition.style().toLowerCase();
    return switch (style) {
      case "success" -> Button.success(definition.id(), definition.label());
      case "danger" -> Button.danger(definition.id(), definition.label());
      case "secondary" -> Button.secondary(definition.id(), definition.label());
      default -> Button.primary(definition.id(), definition.label());
    };
  }
}
