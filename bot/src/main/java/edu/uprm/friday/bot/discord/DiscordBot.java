package edu.uprm.friday.bot.discord;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.backend.dto.BotGuildSummary;
import edu.uprm.friday.bot.config.BotConfig;
import edu.uprm.friday.bot.interactions.EventRouter;
import edu.uprm.friday.bot.interactions.InteractionRegistry;
import net.dv8tion.jda.api.JDA;
import net.dv8tion.jda.api.JDABuilder;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.requests.GatewayIntent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.EnumSet;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public final class DiscordBot {
  private static final Logger LOGGER = LoggerFactory.getLogger(DiscordBot.class);

  private final BotConfig config;
  private final InteractionRegistry interactionRegistry;
  private final ExecutorService eventExecutor;

  public DiscordBot(BotConfig config, InteractionRegistry interactionRegistry) {
    this.config = config;
    this.interactionRegistry = interactionRegistry;
    this.eventExecutor = Executors.newSingleThreadExecutor(runnable -> {
      Thread thread = new Thread(runnable, "friday-discord-events");
      thread.setDaemon(false);
      return thread;
    });
  }

  public JDA start() throws InterruptedException {
    JDA jda = JDABuilder.create(config.discordBotToken(), intents())
      .setEventPool(eventExecutor, true)
      .addEventListeners(new EventRouter(interactionRegistry))
      .build()
      .awaitReady();

    syncCommands(jda);
    Runtime.getRuntime().addShutdownHook(new Thread(jda::shutdown, "discord-shutdown"));
    LOGGER.info("Friday bot is online as {}", jda.getSelfUser().getAsTag());
    return jda;
  }

  private EnumSet<GatewayIntent> intents() {
    return EnumSet.of(
      GatewayIntent.GUILD_MEMBERS,
      GatewayIntent.GUILD_MESSAGES,
      GatewayIntent.MESSAGE_CONTENT);
  }

  private void syncCommands(JDA jda) {
    List<BotGuildSummary> backendGuilds = BackendClient.enabledGuilds();
    if (!backendGuilds.isEmpty()) {
      backendGuilds.forEach(summary -> syncGuildCommands(jda, summary.guildId()));
      return;
    }

    jda.getGuilds().stream()
      .filter(guild -> BackendClient.isGuildEnabled(guild.getIdLong()))
      .forEach(guild -> syncGuildCommands(jda, guild.getIdLong()));
    LOGGER.info("Queued command sync for visible backend-enabled guilds");
  }

  private void syncGuildCommands(JDA jda, long guildId) {
    Guild guild = jda.getGuildById(guildId);
    if (guild == null) {
      LOGGER.warn("Guild {} is configured but unavailable to this bot", guildId);
      return;
    }

    guild.updateCommands().addCommands(interactionRegistry.commandData()).queue();
    LOGGER.info("Queued guild sync for {} Discord commands in guild {}", interactionRegistry.commandCount(), guildId);
  }
}
