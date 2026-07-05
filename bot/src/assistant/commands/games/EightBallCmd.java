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

public final class EightBallCmd extends InteractionModel implements CommandI {
  @Override
  public boolean isGlobal() {
    return true;
  }

  @Override
  public void setGlobal(boolean isGlobal) {}

  private static final List<String> ANSWERS =
      List.of(
          "It is certain.",
          "Signs point to yes.",
          "Without a doubt.",
          "Ask again later.",
          "Better not tell you now.",
          "Cannot predict now.",
          "Do not count on it.",
          "My sources say no.",
          "Very doubtful.");

  @Override
  public String getCommandName() {
    return "game-eightball";
  }

  @Override
  public String getDescription() {
    return "Ask the magic 8-ball a question";
  }

  @Override
  public List<OptionData> getOptions(Guild guild) {
    return List.of(new OptionData(OptionType.STRING, "question", "Your question", true));
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    String question = event.getOption("question").getAsString();
    String answer = ANSWERS.get(ThreadLocalRandom.current().nextInt(ANSWERS.size()));
    event
        .replyEmbeds(
            new EmbedBuilder()
                .setColor(0x2D1B69)
                .setAuthor(
                    event.getUser().getEffectiveName(),
                    null,
                    event.getUser().getEffectiveAvatarUrl())
                .setTitle("🎱 Magic 8-Ball")
                .setDescription("> " + question + "\n\n## " + answer)
                .setFooter("The 8-ball offers guidance, not guarantees.")
                .setTimestamp(Instant.now())
                .build())
        .queue();
  }
}
