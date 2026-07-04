package edu.uprm.friday.bot;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.commands.CommandCatalog;
import edu.uprm.friday.bot.config.BotConfig;
import edu.uprm.friday.bot.discord.DiscordBot;
import edu.uprm.friday.bot.embeds.EmbedFactory;
import edu.uprm.friday.bot.interactions.InteractionRegistry;

public final class FridayBotApplication {
  private FridayBotApplication() {
  }

  public static void main(String[] args) throws InterruptedException {
    BotConfig config = BotConfig.load();

    if (!config.hasUsableToken()) {
      System.err.println("DISCORD_BOT_TOKEN is not set. Bot will stay idle until a token is provided.");
      Thread.currentThread().join();
      return;
    }

    BackendClient.configure(config.backendUrl());
    EmbedFactory embedFactory = new EmbedFactory();
    InteractionRegistry registry = CommandCatalog.createDefault(embedFactory);

    new DiscordBot(config, registry).start();
  }
}
