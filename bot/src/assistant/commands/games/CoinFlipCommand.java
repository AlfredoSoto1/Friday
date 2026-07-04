package assistant.commands.games;

import assistant.backend.BackendClient;
import assistant.backend.dto.BotGuildProfile;
import assistant.app.embeds.EmbedFactory;
import assistant.app.interactions.InteractionDefinition;
import assistant.app.interactions.SlashCommandDefinition;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;
import net.dv8tion.jda.api.interactions.commands.build.Commands;

import java.util.concurrent.ThreadLocalRandom;

public final class CoinFlipCommand extends InteractionDefinition implements SlashCommandDefinition {
  private final EmbedFactory embedFactory;

  public CoinFlipCommand(EmbedFactory embedFactory) {
    this.embedFactory = embedFactory;
  }

  @Override
  public CommandData commandData() {
    return Commands.slash("coinflip", "Play a quick coin flip game.");
  }

  @Override
  public void handle(SlashCommandInteractionEvent event) {
    long guildId = event.getGuild() == null ? 0 : event.getGuild().getIdLong();
    BotGuildProfile profile = BackendClient.guildProfile(guildId);
    String result = ThreadLocalRandom.current().nextBoolean() ? "Heads" : "Tails";

    event.replyEmbeds(embedFactory.gameEmbed(profile, "Coin flip", "The coin landed on **" + result + "**."))
      .setEphemeral(false)
      .queue();
  }
}
