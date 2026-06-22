package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.backend.dto.BotGuildProfile;
import edu.uprm.friday.bot.embeds.EmbedFactory;
import edu.uprm.friday.bot.interactions.InteractionDefinition;
import edu.uprm.friday.bot.interactions.SlashCommandDefinition;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;
import net.dv8tion.jda.api.interactions.commands.build.Commands;

import java.util.concurrent.ThreadLocalRandom;

public final class CoinFlipCommand extends InteractionDefinition implements SlashCommandDefinition {
  private final BackendClient backendClient;
  private final EmbedFactory embedFactory;

  public CoinFlipCommand(BackendClient backendClient, EmbedFactory embedFactory) {
    this.backendClient = backendClient;
    this.embedFactory = embedFactory;
  }

  @Override
  public CommandData commandData() {
    return Commands.slash("coinflip", "Play a quick coin flip game.");
  }

  @Override
  public void handle(SlashCommandInteractionEvent event) {
    long guildId = event.getGuild() == null ? 0 : event.getGuild().getIdLong();
    BotGuildProfile profile = backendClient.guildProfile(guildId);
    String result = ThreadLocalRandom.current().nextBoolean() ? "Heads" : "Tails";

    event.replyEmbeds(embedFactory.gameEmbed(profile, "Coin flip", "The coin landed on **" + result + "**."))
      .setEphemeral(false)
      .queue();
  }
}
