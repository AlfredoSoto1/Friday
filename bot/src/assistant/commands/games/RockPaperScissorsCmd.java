package assistant.commands.games;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import java.time.Instant;
import java.util.List;
import java.util.concurrent.ThreadLocalRandom;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

public final class RockPaperScissorsCmd extends InteractionModel implements CommandI {
  private static final List<String> MOVES = List.of("rock", "paper", "scissors");

  @Override
  public boolean isGlobal() {
    return true;
  }

  @Override
  public void setGlobal(boolean isGlobal) {}

  @Override
  public String getCommandName() {
    return "game-rps";
  }

  @Override
  public String getDescription() {
    return "Play rock, paper, scissors against Friday";
  }

  @Override
  public List<OptionData> getOptions(Guild guild) {
    return List.of(
        new OptionData(OptionType.STRING, "move", "Choose your move", true)
            .addChoice("🪨 Rock", "rock")
            .addChoice("📄 Paper", "paper")
            .addChoice("✂️ Scissors", "scissors"));
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    String player = event.getOption("move").getAsString();
    String bot = MOVES.get(ThreadLocalRandom.current().nextInt(MOVES.size()));
    int outcome = (MOVES.indexOf(player) - MOVES.indexOf(bot) + 3) % 3;
    String verdict = outcome == 0 ? "Draw" : outcome == 1 ? "You win!" : "Assistant wins";
    int color = outcome == 1 ? 0x27AE60 : outcome == 2 ? 0xEB5757 : 0x56CCF2;
    event
        .replyEmbeds(
            new EmbedBuilder()
                .setColor(color)
                .setAuthor(
                    event.getUser().getEffectiveName(),
                    null,
                    event.getUser().getEffectiveAvatarUrl())
                .setTitle("⚔️ Rock • Paper • Scissors")
                .setDescription(
                    "**You chose:** "
                        + symbol(player)
                        + " "
                        + player
                        + "\n"
                        + "**Assistant chose:** "
                        + symbol(bot)
                        + " "
                        + bot
                        + "\n\n## "
                        + verdict)
                .setTimestamp(Instant.now())
                .build())
        .queue();
  }

  private String symbol(String move) {
    return switch (move) {
      case "rock" -> "🪨";
      case "paper" -> "📄";
      default -> "✂️";
    };
  }
}
