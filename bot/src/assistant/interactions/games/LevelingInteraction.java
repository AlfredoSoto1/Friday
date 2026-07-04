package assistant.interactions.games;

import assistant.backend.BackendClient;
import assistant.backend.dto.BotGuildProfile;
import assistant.backend.dto.BotXpRequest;
import assistant.backend.dto.BotXpResult;
import assistant.app.embeds.EmbedFactory;
import assistant.app.interactions.InteractionDefinition;
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
