package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.backend.dto.BotGuildProfile;
import edu.uprm.friday.bot.backend.dto.BotXpRequest;
import edu.uprm.friday.bot.backend.dto.BotXpResult;
import edu.uprm.friday.bot.embeds.EmbedFactory;
import edu.uprm.friday.bot.interactions.InteractionDefinition;
import net.dv8tion.jda.api.events.message.MessageReceivedEvent;

public final class LevelingInteraction extends InteractionDefinition {
  private final BackendClient backendClient;
  private final EmbedFactory embedFactory;

  public LevelingInteraction(BackendClient backendClient, EmbedFactory embedFactory) {
    this.backendClient = backendClient;
    this.embedFactory = embedFactory;
  }

  @Override
  public void onMessageReceived(MessageReceivedEvent event) {
    BotXpResult result = backendClient.addXp(event.getGuild().getIdLong(), new BotXpRequest(
      event.getAuthor().getId(),
      event.getAuthor().getName(),
      1));

    if (result.leveledUp()) {
      BotGuildProfile profile = backendClient.guildProfile(event.getGuild().getIdLong());
      event.getChannel().sendMessageEmbeds(embedFactory.levelUpEmbed(
        profile,
        event.getAuthor().getAsMention(),
        result.level())).queue();
    }
  }
}
