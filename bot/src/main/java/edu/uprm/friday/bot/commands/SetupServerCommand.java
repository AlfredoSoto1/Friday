package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.backend.dto.BotSetupChannel;
import edu.uprm.friday.bot.backend.dto.BotSetupProfile;
import edu.uprm.friday.bot.interactions.InteractionDefinition;
import edu.uprm.friday.bot.interactions.SlashCommandDefinition;
import net.dv8tion.jda.api.Permission;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.entities.channel.concrete.Category;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;
import net.dv8tion.jda.api.interactions.commands.build.Commands;

import java.util.Comparator;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;
import java.util.concurrent.CompletableFuture;

public final class SetupServerCommand extends InteractionDefinition implements SlashCommandDefinition {
  private final BackendClient backendClient;

  public SetupServerCommand(BackendClient backendClient) {
    this.backendClient = backendClient;
  }

  @Override
  public CommandData commandData() {
    return Commands.slash("setup-server", "Set up a blank server from the backend server profile.")
      .addOption(OptionType.STRING, "server_id", "Discord server ID to set up.", true);
  }

  @Override
  public void handle(SlashCommandInteractionEvent event) {
    if (event.getMember() == null || !event.getMember().hasPermission(Permission.ADMINISTRATOR)) {
      event.reply("Only server administrators can run this command.").setEphemeral(true).queue();
      return;
    }

    String rawServerId = event.getOption("server_id").getAsString();
    long serverId;
    try {
      serverId = Long.parseLong(rawServerId);
    } catch (NumberFormatException ex) {
      event.reply("The server_id must be a numeric Discord guild ID.").setEphemeral(true).queue();
      return;
    }

    Guild targetGuild = event.getJDA().getGuildById(serverId);
    if (targetGuild == null) {
      event.reply("Friday is not in that server, or the server is not visible to this bot.").setEphemeral(true).queue();
      return;
    }

    event.deferReply(true).queue(hook -> CompletableFuture
      .supplyAsync(() -> setup(targetGuild, backendClient.setupProfile(serverId)))
      .whenComplete((message, throwable) -> {
        if (throwable != null) {
          hook.editOriginal("Server setup failed: " + throwable.getMessage()).queue();
        } else {
          hook.editOriginal(message).queue();
        }
      }));
  }

  private String setup(Guild guild, BotSetupProfile profile) {
    Map<String, Category> categories = new HashMap<>();
    int created = 0;
    int skipped = 0;

    for (BotSetupChannel channel : profile.channels().stream().sorted(Comparator.comparingInt(BotSetupChannel::position)).toList()) {
      Category category = null;
      if (channel.category() != null && !channel.category().isBlank()) {
        category = categories.computeIfAbsent(channel.category(), name -> existingCategory(guild, name)
          .orElseGet(() -> guild.createCategory(name).complete()));
      }

      if ("voice".equalsIgnoreCase(channel.type())) {
        if (guild.getVoiceChannelsByName(channel.name(), true).isEmpty()) {
          guild.createVoiceChannel(channel.name(), category).complete();
          created++;
        } else {
          skipped++;
        }
      } else {
        if (guild.getTextChannelsByName(channel.name(), true).isEmpty()) {
          guild.createTextChannel(channel.name(), category).complete();
          created++;
        } else {
          skipped++;
        }
      }
    }

    return "Server setup complete. Created " + created + " channels and skipped " + skipped + " existing channels.";
  }

  private Optional<Category> existingCategory(Guild guild, String name) {
    return guild.getCategoriesByName(name, true).stream().findFirst();
  }
}
