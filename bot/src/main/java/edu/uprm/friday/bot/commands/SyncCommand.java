package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.backend.dto.BotSyncChannel;
import edu.uprm.friday.bot.backend.dto.BotSyncRequest;
import edu.uprm.friday.bot.backend.dto.BotSyncResult;
import edu.uprm.friday.bot.backend.dto.BotSyncRole;
import edu.uprm.friday.bot.interactions.InteractionDefinition;
import edu.uprm.friday.bot.interactions.SlashCommandDefinition;
import net.dv8tion.jda.api.Permission;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.entities.Role;
import net.dv8tion.jda.api.entities.channel.ChannelType;
import net.dv8tion.jda.api.entities.channel.middleman.GuildChannel;
import net.dv8tion.jda.api.entities.channel.middleman.StandardGuildChannel;
import net.dv8tion.jda.api.entities.channel.middleman.StandardGuildMessageChannel;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.DefaultMemberPermissions;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;
import net.dv8tion.jda.api.interactions.commands.build.Commands;

import java.util.Comparator;
import java.util.List;

public final class SyncCommand extends InteractionDefinition implements SlashCommandDefinition {
  
  public SyncCommand() {
  }

  @Override
  public CommandData commandData() {
    return Commands.slash("sync", "Sync this server's roles and channels into Friday.")
      .setDefaultPermissions(DefaultMemberPermissions.enabledFor(Permission.ADMINISTRATOR));
  }

  @Override
  public void handle(SlashCommandInteractionEvent event) {
    if (event.getGuild() == null || event.getMember() == null) {
      event.reply("/sync can only be used inside a server.").setEphemeral(true).queue();
      return;
    }

    if (!event.getMember().hasPermission(Permission.ADMINISTRATOR)) {
      event.reply("Only server administrators can run /sync.").setEphemeral(true).queue();
      return;
    }

    event.deferReply(true).queue(hook -> {
      BotSyncResult result = BackendClient.syncGuild(toRequest(event.getGuild(), event.getUser().getId()));
      hook.editOriginal("Sync complete. Stored " + result.roleCount() + " roles, "
        + result.channelCount() + " channels, and " + result.categoryCount() + " categories.").queue();
    });
  }

  private BotSyncRequest toRequest(Guild guild, String syncedByDiscordId) {
    List<BotSyncRole> roles = guild.getRoles().stream()
      .sorted(Comparator.comparingInt(Role::getPosition))
      .map(role -> new BotSyncRole(
        role.getId(),
        role.getName(),
        role.getColorRaw(),
        role.getPosition(),
        role.isManaged(),
        role.isMentionable(),
        role.isHoisted()))
      .toList();

    List<BotSyncChannel> channels = guild.getChannels().stream()
      .sorted(Comparator.comparingInt(this::positionOf))
      .map(this::toChannel)
      .toList();

    return new BotSyncRequest(guild.getIdLong(), guild.getName(), syncedByDiscordId, roles, channels);
  }

  private BotSyncChannel toChannel(GuildChannel channel) {
    String topic = channel instanceof StandardGuildMessageChannel messageChannel ? messageChannel.getTopic() : null;
    boolean nsfw = channel instanceof StandardGuildMessageChannel messageChannel && messageChannel.isNSFW();
    String parentId = channel instanceof StandardGuildChannel standardChannel && standardChannel.getParentCategory() != null
      ? standardChannel.getParentCategory().getId()
      : null;

    return new BotSyncChannel(
      channel.getId(),
      parentId,
      channel.getName(),
      typeName(channel.getType()),
      positionOf(channel),
      topic,
      nsfw);
  }

  private int positionOf(GuildChannel channel) {
    return channel instanceof StandardGuildChannel standardChannel ? standardChannel.getPosition() : 0;
  }

  private String typeName(ChannelType type) {
    return switch (type) {
      case CATEGORY -> "category";
      case TEXT -> "text";
      case VOICE -> "voice";
      case STAGE -> "stage";
      case NEWS -> "news";
      case FORUM -> "forum";
      default -> type.name().toLowerCase();
    };
  }
}
