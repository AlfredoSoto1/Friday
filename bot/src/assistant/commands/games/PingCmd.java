package assistant.commands.games;

import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;

public final class PingCmd extends GameCommand {
  @Override
  public String getCommandName() {
    return "ping";
  }

  @Override
  public String getDescription() {
    return "Check the bot gateway latency";
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    long latency = event.getJDA().getGatewayPing();
    int color = latency < 150 ? 0x27AE60 : latency < 300 ? 0xF2C94C : 0xEB5757;
    String status = latency < 150 ? "Excellent" : latency < 300 ? "Stable" : "Degraded";
    event
        .replyEmbeds(
            embed.result(
                "🏓 Pong!",
                "**Gateway latency**\n# " + latency + " ms\n`" + status + " connection`",
                color,
                event.getUser().getEffectiveName(),
                event.getUser().getEffectiveAvatarUrl()))
        .queue();
  }
}
