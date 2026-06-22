package edu.uprm.friday.bot.discord;

import edu.uprm.friday.bot.commands.CommandRegistry;
import edu.uprm.friday.bot.config.BotConfig;
import net.dv8tion.jda.api.JDA;
import net.dv8tion.jda.api.JDABuilder;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.hooks.ListenerAdapter;
import net.dv8tion.jda.api.requests.GatewayIntent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.EnumSet;

public final class DiscordBot {
    private static final Logger LOGGER = LoggerFactory.getLogger(DiscordBot.class);

    private final BotConfig config;
    private final CommandRegistry commandRegistry;

    public DiscordBot(BotConfig config, CommandRegistry commandRegistry) {
        this.config = config;
        this.commandRegistry = commandRegistry;
    }

    public JDA start() throws InterruptedException {
        JDA jda = JDABuilder.createLight(config.discordBotToken(), EnumSet.noneOf(GatewayIntent.class))
                .addEventListeners(new CommandInteractionListener(commandRegistry))
                .build()
                .awaitReady();

        syncCommands(jda);
        Runtime.getRuntime().addShutdownHook(new Thread(jda::shutdown, "discord-shutdown"));
        LOGGER.info("Friday bot is online as {}", jda.getSelfUser().getAsTag());
        return jda;
    }

    private void syncCommands(JDA jda) {
        config.discordGuildId().ifPresentOrElse(
                guildId -> syncGuildCommands(jda, guildId),
                () -> {
                    jda.updateCommands().addCommands(commandRegistry.commandData()).queue();
                    LOGGER.info("Queued global sync for {} Discord commands", commandRegistry.size());
                }
        );
    }

    private void syncGuildCommands(JDA jda, long guildId) {
        Guild guild = jda.getGuildById(guildId);
        if (guild == null) {
            LOGGER.warn("DISCORD_GUILD_ID={} is not available to this bot; falling back to global command sync", guildId);
            jda.updateCommands().addCommands(commandRegistry.commandData()).queue();
            return;
        }

        guild.updateCommands().addCommands(commandRegistry.commandData()).queue();
        LOGGER.info("Queued guild sync for {} Discord commands in guild {}", commandRegistry.size(), guildId);
    }

    private static final class CommandInteractionListener extends ListenerAdapter {
        private final CommandRegistry commandRegistry;

        private CommandInteractionListener(CommandRegistry commandRegistry) {
            this.commandRegistry = commandRegistry;
        }

        @Override
        public void onSlashCommandInteraction(SlashCommandInteractionEvent event) {
            commandRegistry.handle(event);
        }
    }
}
