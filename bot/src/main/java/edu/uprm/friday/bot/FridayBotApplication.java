package edu.uprm.friday.bot;

import edu.uprm.friday.bot.commands.CommandRegistry;
import edu.uprm.friday.bot.config.BotConfig;
import edu.uprm.friday.bot.discord.DiscordBot;

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

        DiscordBot bot = new DiscordBot(config, CommandRegistry.createDefault());
        bot.start();
    }
}
