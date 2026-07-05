package assistant.commands.games;

import java.util.List;
import java.util.concurrent.ThreadLocalRandom;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

public final class EightBallCmd extends GameCommand {
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
    return "eightball";
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
            embed.resultWithFooter(
                "🎱 Magic 8-Ball",
                "> " + question + "\n\n## " + answer,
                0x2D1B69,
                event.getUser().getEffectiveName(),
                event.getUser().getEffectiveAvatarUrl(),
                "The 8-ball offers guidance, not guarantees."))
        .queue();
  }
}
