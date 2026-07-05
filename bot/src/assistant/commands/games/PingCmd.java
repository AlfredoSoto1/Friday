package assistant.commands.games;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import java.time.Instant;
import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

public final class PingCmd extends InteractionModel implements CommandI {
  @Override
  public boolean isGlobal() {
    return true;
  }

  @Override
  public void setGlobal(boolean isGlobal) {
  }

  @Override
  public String getCommandName() {
    return "game-ping";
  }

  @Override
  public String getDescription() {
    return "Check the bot gateway latency";
  }

  @Override
  public List<OptionData> getOptions(Guild guild) {
    return List.of();
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    long latency = event.getJDA().getGatewayPing();
    int color = latency < 150 ? 0x27AE60 : latency < 300 ? 0xF2C94C : 0xEB5757;
    String status = latency < 150 ? "Excellent" : latency < 300 ? "Stable" : "Degraded";
    event
        .replyEmbeds(
            new EmbedBuilder()
                .setColor(color)
                .setTitle("🏓 Pong!")
                .setDescription(
                    "**Gateway latency**\n# " + latency + " ms\n`" + status + " connection`")
                .setTimestamp(Instant.now())
                .build())
        .queue();
  }
}
