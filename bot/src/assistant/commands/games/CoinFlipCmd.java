package assistant.commands.games;

import java.util.concurrent.ThreadLocalRandom;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;

public final class CoinFlipCmd extends GameCommand {
  @Override
  public String getCommandName() {
    return "game-coinflip";
  }

  @Override
  public String getDescription() {
    return "Flip a coin";
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    boolean heads = ThreadLocalRandom.current().nextBoolean();
    String result = heads ? "Heads" : "Tails";
    String icon = heads ? "🟡" : "⚪";
    event
        .replyEmbeds(
            embed.result(
                icon + " Coin Flip",
                "## " + result + "\nThe coin has spoken.",
                heads ? 0xF2C94C : 0xB8C2CC,
                event.getUser().getEffectiveName(),
                event.getUser().getEffectiveAvatarUrl()))
        .queue();
  }
}
