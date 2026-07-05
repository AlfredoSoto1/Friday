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

public final class DiceCmd extends InteractionModel implements CommandI {
  @Override
  public boolean isGlobal() {
    return true;
  }

  @Override
  public void setGlobal(boolean isGlobal) {}

  @Override
  public String getCommandName() {
    return "game-dice";
  }

  @Override
  public String getDescription() {
    return "Roll a custom-sided die";
  }

  @Override
  public List<OptionData> getOptions(Guild guild) {
    return List.of(
        new OptionData(OptionType.INTEGER, "sides", "Number of sides (defaults to 6)", false)
            .setMinValue(2)
            .setMaxValue(100));
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    int sides = event.getOption("sides") == null ? 6 : event.getOption("sides").getAsInt();
    int roll = ThreadLocalRandom.current().nextInt(1, sides + 1);
    event
        .replyEmbeds(
            new EmbedBuilder()
                .setColor(0x9B51E0)
                .setAuthor(
                    event.getUser().getEffectiveName(),
                    null,
                    event.getUser().getEffectiveAvatarUrl())
                .setTitle("🎲 Dice Roll")
                .setDescription("You rolled\n# " + roll)
                .setFooter("d" + sides + " • Range 1–" + sides)
                .setTimestamp(Instant.now())
                .build())
        .queue();
  }
}
