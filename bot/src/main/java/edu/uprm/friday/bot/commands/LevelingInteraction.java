package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.backend.dto.BotGuildProfile;
import edu.uprm.friday.bot.backend.dto.BotXpRequest;
import edu.uprm.friday.bot.backend.dto.BotXpResult;
import edu.uprm.friday.bot.embeds.EmbedFactory;
import edu.uprm.friday.bot.interactions.InteractionDefinition;
import net.dv8tion.jda.api.events.message.MessageReceivedEvent;

public final class LevelingInteraction extends InteractionDefinition {
  private final EmbedFactory embedFactory;

  public LevelingInteraction(EmbedFactory embedFactory) {
    this.embedFactory = embedFactory;
  }

  @Override
  public void onMessageReceived(MessageReceivedEvent event) {
    BotXpResult result = BackendClient.addXp(event.getGuild().getIdLong(), new BotXpRequest(
      event.getAuthor().getId()));

    if (result.leveledUp()) {
      BotGuildProfile profile = BackendClient.guildProfile(event.getGuild().getIdLong());
      event.getChannel().sendMessageEmbeds(embedFactory.levelUpEmbed(
        profile,
        event.getAuthor().getAsMention(),
        result.level())).queue();
    }
  }
}
