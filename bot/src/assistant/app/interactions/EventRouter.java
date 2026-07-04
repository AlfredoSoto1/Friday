package assistant.app.interactions;

import assistant.backend.BackendClient;
import net.dv8tion.jda.api.events.guild.member.GuildMemberJoinEvent;
import net.dv8tion.jda.api.events.interaction.ModalInteractionEvent;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.events.interaction.component.ButtonInteractionEvent;
import net.dv8tion.jda.api.events.interaction.component.StringSelectInteractionEvent;
import net.dv8tion.jda.api.events.message.MessageReceivedEvent;
import net.dv8tion.jda.api.events.session.ReadyEvent;
import net.dv8tion.jda.api.hooks.ListenerAdapter;

public final class EventRouter extends ListenerAdapter {
    private final InteractionRegistry registry;

  public EventRouter(InteractionRegistry registry) {
    this.registry = registry;
  }

  @Override
  public void onReady(ReadyEvent event) {
    registry.interactions().forEach(interaction -> interaction.onReady(event));
    event.getJDA().getGuilds().forEach(guild -> registry.interactions()
      .forEach(interaction -> interaction.onGuildReady(guild)));
  }

  @Override
  public void onSlashCommandInteraction(SlashCommandInteractionEvent event) {
    if (!guildEnabled(event.getGuild() == null ? 0 : event.getGuild().getIdLong())) {
      event.reply("Friday is not enabled for this server.").setEphemeral(true).queue();
      return;
    }

    registry.command(event.getName()).ifPresentOrElse(
      command -> command.handle(event),
      () -> event.reply("The command /" + event.getName() + " is not registered.").setEphemeral(true).queue());
  }

  @Override
  public void onButtonInteraction(ButtonInteractionEvent event) {
    registry.button(event.getComponentId()).ifPresentOrElse(
      handler -> handler.handle(event),
      () -> event.reply("This button is not registered.").setEphemeral(true).queue());
  }

  @Override
  public void onModalInteraction(ModalInteractionEvent event) {
    registry.modal(event.getModalId()).ifPresentOrElse(
      handler -> handler.handle(event),
      () -> event.reply("This modal is not registered.").setEphemeral(true).queue());
  }

  @Override
  public void onStringSelectInteraction(StringSelectInteractionEvent event) {
    registry.selectMenu(event.getComponentId()).ifPresentOrElse(
      handler -> handler.handle(event),
      () -> event.reply("This select menu is not registered.").setEphemeral(true).queue());
  }

  @Override
  public void onGuildMemberJoin(GuildMemberJoinEvent event) {
    if (event.getUser().isBot() || !guildEnabled(event.getGuild().getIdLong())) {
      return;
    }
    registry.interactions().forEach(interaction -> interaction.onMemberJoin(event));
  }

  @Override
  public void onMessageReceived(MessageReceivedEvent event) {
    if (event.getAuthor().isBot() || !event.isFromGuild() || !guildEnabled(event.getGuild().getIdLong())) {
      return;
    }
    registry.interactions().forEach(interaction -> interaction.onMessageReceived(event));
  }

  private boolean guildEnabled(long guildId) {
    return guildId == 0 || BackendClient.isGuildEnabled(guildId);
  }
}
