package assistant.commands.games;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import java.time.Instant;
import java.util.List;
import java.util.concurrent.ThreadLocalRandom;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

public final class CoinFlipCmd extends InteractionModel implements CommandI {
  @Override
  public boolean isGlobal() {
    return true;
  }

  @Override
  public void setGlobal(boolean isGlobal) {}

  @Override
  public String getCommandName() {
    return "game-coinflip";
  }

  @Override
  public String getDescription() {
    return "Flip a coin";
  }

  @Override
  public List<OptionData> getOptions(Guild guild) {
    return List.of();
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    boolean heads = ThreadLocalRandom.current().nextBoolean();
    String result = heads ? "Heads" : "Tails";
    event
        .replyEmbeds(
            new EmbedBuilder()
                .setColor(heads ? 0xF2C94C : 0xB8C2CC)
                .setAuthor(
                    event.getUser().getEffectiveName(),
                    null,
                    event.getUser().getEffectiveAvatarUrl())
                .setTitle((heads ? "🟡" : "⚪") + " Coin Flip")
                .setDescription("# " + result + "\nThe coin has spoken.")
                .setTimestamp(Instant.now())
                .build())
        .queue();
  }
}
