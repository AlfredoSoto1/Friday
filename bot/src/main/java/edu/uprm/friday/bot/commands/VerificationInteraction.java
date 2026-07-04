package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.backend.dto.BotGuildProfile;
import edu.uprm.friday.bot.backend.dto.BotVerifyMemberRequest;
import edu.uprm.friday.bot.backend.dto.BotVerifyMemberResult;
import edu.uprm.friday.bot.embeds.EmbedFactory;
import edu.uprm.friday.bot.interactions.InteractionDefinition;
import edu.uprm.friday.bot.interactions.SlashCommandDefinition;
import net.dv8tion.jda.api.Permission;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.entities.Role;
import net.dv8tion.jda.api.entities.channel.concrete.TextChannel;
import net.dv8tion.jda.api.events.guild.member.GuildMemberJoinEvent;
import net.dv8tion.jda.api.events.interaction.ModalInteractionEvent;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.events.interaction.component.ButtonInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;
import net.dv8tion.jda.api.interactions.commands.build.Commands;
import net.dv8tion.jda.api.interactions.components.ActionRow;
import net.dv8tion.jda.api.interactions.components.buttons.Button;
import net.dv8tion.jda.api.interactions.components.text.TextInput;
import net.dv8tion.jda.api.interactions.components.text.TextInputStyle;
import net.dv8tion.jda.api.interactions.modals.Modal;

import java.util.Objects;

public final class VerificationInteraction extends InteractionDefinition implements SlashCommandDefinition {
  private static final String VERIFY_BUTTON_ID = "friday:verification:start";
  private static final String VERIFY_MODAL_ID = "friday:verification:modal";
  private static final String EMAIL_INPUT_ID = "email";
  private static final String FUN_FACT_INPUT_ID = "fun_fact";

  private final EmbedFactory embedFactory;

  public VerificationInteraction(EmbedFactory embedFactory) {
    this.embedFactory = embedFactory;
    registerButton(VERIFY_BUTTON_ID, this::showVerificationModal);
    registerModal(VERIFY_MODAL_ID, this::verifyMember);
  }

  @Override
  public CommandData commandData() {
    return Commands.slash("verification-panel", "Send the verification embed to a server channel.")
      .addOption(OptionType.CHANNEL, "channel", "Channel where the verification panel should be sent.", false);
  }

  @Override
  public void handle(SlashCommandInteractionEvent event) {
    if (event.getMember() == null || !event.getMember().hasPermission(Permission.ADMINISTRATOR)) {
      event.reply("Only server administrators can send the verification panel.").setEphemeral(true).queue();
      return;
    }

    Guild guild = Objects.requireNonNull(event.getGuild());
    BotGuildProfile profile = BackendClient.guildProfile(guild.getIdLong());
    TextChannel channel = event.getOption("channel") == null
      ? configuredVerificationChannel(guild, profile)
      : event.getOption("channel").getAsChannel().asTextChannel();

    if (channel == null) {
      event.reply("No verification channel is configured or selected.").setEphemeral(true).queue();
      return;
    }

    channel.sendMessageEmbeds(embedFactory.verificationEmbed(profile))
      .setActionRow(Button.success(VERIFY_BUTTON_ID, profile.verification().buttonLabel()))
      .queue();
    event.reply("Verification panel sent to " + channel.getAsMention() + ".").setEphemeral(true).queue();
  }

  @Override
  public void onMemberJoin(GuildMemberJoinEvent event) {
    BotGuildProfile profile = BackendClient.guildProfile(event.getGuild().getIdLong());
    if (!profile.welcome().enabled()) {
      return;
    }

    if (profile.welcome().channelId() != null) {
      TextChannel channel = event.getGuild().getTextChannelById(profile.welcome().channelId());
      if (channel != null) {
        channel.sendMessage(event.getMember().getAsMention())
          .addEmbeds(embedFactory.welcomeEmbed(profile))
          .queue();
      }
    }

    event.getUser().openPrivateChannel().queue(channel -> channel.sendMessageEmbeds(embedFactory.verificationEmbed(profile))
      .setActionRow(Button.success(VERIFY_BUTTON_ID, profile.verification().buttonLabel()))
      .queue());
  }

  private void showVerificationModal(ButtonInteractionEvent event) {
    TextInput email = TextInput.create(EMAIL_INPUT_ID, "Institutional email", TextInputStyle.SHORT)
      .setPlaceholder("your.email@upr.edu")
      .setRequired(true)
      .setMinLength(6)
      .setMaxLength(255)
      .build();

    TextInput funFact = TextInput.create(FUN_FACT_INPUT_ID, "Fun fact", TextInputStyle.PARAGRAPH)
      .setPlaceholder("Tell us something about you.")
      .setRequired(false)
      .setMaxLength(255)
      .build();

    event.replyModal(Modal.create(VERIFY_MODAL_ID, "Member verification")
      .addComponents(ActionRow.of(email), ActionRow.of(funFact))
      .build()).queue();
  }

  private void verifyMember(ModalInteractionEvent event) {
    if (event.getGuild() == null || event.getMember() == null) {
      event.reply("Verification must happen inside a server context.").setEphemeral(true).queue();
      return;
    }

    String email = event.getValue(EMAIL_INPUT_ID).getAsString();
    String funFact = event.getValue(FUN_FACT_INPUT_ID) == null ? null : event.getValue(FUN_FACT_INPUT_ID).getAsString();
    BotVerifyMemberResult result = BackendClient.verifyMember(event.getGuild().getIdLong(), new BotVerifyMemberRequest(
      event.getUser().getId(),
      email,
      funFact));

    if (result.verified()) {
      for (String roleId : result.roleIds()) {
        Role role = event.getGuild().getRoleById(roleId);
        if (role != null) {
          event.getGuild().addRoleToMember(event.getMember(), role).queue();
        }
      }
    }

    event.reply(result.message()).setEphemeral(true).queue();
  }

  private TextChannel configuredVerificationChannel(Guild guild, BotGuildProfile profile) {
    if (profile.verification().channelId() != null) {
      return guild.getTextChannelById(profile.verification().channelId());
    }
    return guild.getTextChannelsByName("verification", true).stream().findFirst().orElse(null);
  }
}
