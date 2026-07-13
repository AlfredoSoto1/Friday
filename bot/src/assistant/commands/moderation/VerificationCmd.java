/*
 * Copyright 2024 Alfredo Soto
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package assistant.commands.moderation;

import assistant.app.discord.Logger;
import assistant.app.discord.Logger.LogFeedback;
import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.app.model.MemberPosition;
import assistant.backend.dto.BotVerifyMemberResult;
import assistant.backend.dto.DiscordRoleDTO;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.dto.MemberDTO;
import assistant.backend.dto.PrepaOrientadorDTO;
import assistant.backend.dto.TeamDTO;
import assistant.backend.service.MemberService;
import assistant.embeds.moderation.VerificationEmbed;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.entities.Member;
import net.dv8tion.jda.api.entities.Role;
import net.dv8tion.jda.api.entities.channel.concrete.PrivateChannel;
import net.dv8tion.jda.api.entities.channel.concrete.TextChannel;
import net.dv8tion.jda.api.events.interaction.ModalInteractionEvent;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.events.interaction.component.ButtonInteractionEvent;
import net.dv8tion.jda.api.exceptions.HierarchyException;
import net.dv8tion.jda.api.interactions.InteractionHook;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;
import net.dv8tion.jda.api.interactions.components.ActionRow;
import net.dv8tion.jda.api.interactions.components.buttons.Button;
import net.dv8tion.jda.api.interactions.components.text.TextInput;
import net.dv8tion.jda.api.interactions.components.text.TextInputStyle;
import net.dv8tion.jda.api.interactions.modals.Modal;

/**
 * @author Alfredo
 */
public class VerificationCmd extends InteractionModel implements CommandI {

  private static final String COMMAND_LABEL = "channel";

  private Modal verifyPrompt;
  private Button verifyButton;

  private MemberService service;
  private VerificationEmbed verificationEmbed;

  public VerificationCmd() {
    this.verificationEmbed = new VerificationEmbed();
    this.service = new MemberService();

    // Create an Email field to be displayed inside the modal
    TextInput email = TextInput.create("email-id", "Email", TextInputStyle.SHORT)
        .setMinLength(9)
        .setMaxLength(100)
        .setRequired(true)
        .setPlaceholder("your.email@upr.edu")
        .build();

    // Create a FunFacts field to be displayed inside the modal
    TextInput funfacts = TextInput.create("funfact-id", "Fun facts about you :)", TextInputStyle.PARAGRAPH)
        .setMinLength(1)
        .setMaxLength(255)
        .setRequired(false)
        .setPlaceholder("Tell us more about you!")
        .build();

    // Create a simple modal containing two text fields
    // in which the user will enter his email to log-in and
    // a fun fact about them
    super.registerModal(
        this::onModalVerificationRespond,
        verifyPrompt = Modal.create("mem-verification", "Member Verification")
            .addComponents(ActionRow.of(email), ActionRow.of(funfacts))
            .build());

    super.registerButton(
        this::onVerificationEvent, verifyButton = Button.success("verification-button", "verify"));
  }

  @Override
  public void onDispose() {
    service.shutdownVerificationQueueService();
  }

  @Override
  public boolean isGlobal() {
    return false;
  }

  @Override
  @Deprecated
  public void setGlobal(boolean isGlobal) {
    // This is a server only command
  }

  @Override
  public String getCommandName() {
    return "assistant-verification";
  }

  @Override
  public String getDescription() {
    return "Sends an embed to verify user to a channel of choice";
  }

  @Override
  public List<OptionData> getOptions(Guild server) {
    return List.of(
        new OptionData(
            OptionType.STRING, COMMAND_LABEL, "select channel to send verification message", true));
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {

    if (!super.validateCommandUse(event))
      return;

    String logChannel = event.getOption(COMMAND_LABEL).getAsString();

    try {
      Long.parseLong(logChannel);
    } catch (NumberFormatException nfe) {
      event
          .reply("The id provided for the verification-channel is not a valid number")
          .setEphemeral(true)
          .queue();
      return;
    }

    // Check if input was given successfully
    Optional<TextChannel> textChannel = Optional.ofNullable(event.getGuild().getTextChannelById(logChannel));

    // Check if the channel is in server
    if (textChannel.isPresent()) {
      event.deferReply(true).queue(hook -> {
        Optional<Role> modRole = super.getEffectiveRole(MemberPosition.MODERATOR, textChannel.get().getGuild());
        Optional<Role> bdeRole = super.getEffectiveRole(MemberPosition.BOT_DEVELOPER, textChannel.get().getGuild());
        if (modRole.isEmpty() || bdeRole.isEmpty()) {
          hook.sendMessage("Configure the MODERATOR and BOT_DEVELOPER role mappings first.")
              .setEphemeral(true)
              .queue();
          return;
        }

        DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
        int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);

        textChannel
            .get()
            .sendMessageEmbeds(
                verificationEmbed.buildVerificationPrompt(modRole.get(), bdeRole.get(), color))
            .setActionRow(verifyButton)
            .queue(
                message -> hook.sendMessage("Verification embed sent to: " + textChannel.get().getName())
                    .setEphemeral(true)
                    .queue(),
                error -> hook.sendMessage("The verification embed could not be sent. Check my channel permissions.")
                    .setEphemeral(true)
                    .queue());
      });
    } else
      event.reply("Channel not found").setEphemeral(true).queue();
  }

  private void onVerificationEvent(ButtonInteractionEvent event) {
    // Show the verification modal to the user
    event.replyModal(verifyPrompt).queue();
  }

  private void onModalVerificationRespond(ModalInteractionEvent event) {
    // Respond to the user (ephemeral response)
    event
        .reply("Estamos verificando tu correo y preparando tus roles.")
        .setEphemeral(true)
        .queue();

    // Do this asynchronously
    service.queueVerificationTask(event, this::verifyUser);
  }

  private void verifyUser(ModalInteractionEvent event) {
    String email = event.getValue("email-id").getAsString().trim();
    String funFact = Optional.ofNullable(event.getValue("funfact-id"))
        .map(value -> value.getAsString().trim())
        .filter(value -> !value.isBlank())
        .orElse(null);

    BotVerifyMemberResult verification = service.verifyMember(
        event.getGuild().getIdLong(), email, event.getUser().getId(), funFact);
    if (!verification.verified()) {
      event.getHook().sendMessage(verification.message()).setEphemeral(true).queue();
      Logger.instance().logFile(
          LogFeedback.INFO, "Failed verifying member: %s", event.getMember().getEffectiveName());
      return;
    }

    applyRoles(event.getHook(), event.getGuild(), event.getMember(), verification.roleIds());
    service.getMember(email).ifPresent(member ->
        applyNickname(event.getHook(), event.getGuild(), event.getMember(), member));
    event.getHook().sendMessage(verification.message()).setEphemeral(true).queue();
  }

  private void showWelcomeMessage(PrivateChannel privateChannel, MemberDTO member, Guild server) {

    // Check if the member is an orientador
    if (service.isOrientador(member.getEmail(), server.getIdLong())) {
      // Send custom message to orientador is joining the server
      sendWelcomeMessageToOrientador(privateChannel, server, member);
      return;
    }

    // Obtain the team of the member
    Optional<TeamDTO> team = service.getMemberTeam(member.getEmail(), server.getIdLong());

    // If team not found, no need to print or display any message
    // since its handled when the team is applied to the member
    if (team.isEmpty())
      return;

    // Send welcome message to member if its a prepa
    sendWelcomeMessageToPrepa(privateChannel, server, member, team.get());
  }

  private String emojiMention(Guild server, String name, String fallback) {
    return server.getEmojisByName(name, true).stream()
        .findFirst()
        .map(emoji -> emoji.getAsMention())
        .orElse(fallback);
  }

  private void sendWelcomeMessageToPrepa(
      PrivateChannel privateChannel, Guild server, MemberDTO member, TeamDTO team) {
    // Obtain discord server information
    DiscordServerDTO discordServer = super.getServerOwnerInfo(server.getIdLong());

    // Obtain a list of all the orientadores that form part of
    // the same group as the prepa member who is joining
    List<PrepaOrientadorDTO> prepaOrientadors = service.getPrepaOrientadores(member.getEmail(), server.getIdLong());

    // Obtain the names of the orientadores assuming
    // that the member joining is a prepa and is already a verified member
    String orientadorNames = prepaOrientadors.stream()
        .map(PrepaOrientadorDTO::getFirstname)
        .collect(Collectors.joining(", "));

    privateChannel
        .sendMessage(
            String.format(
                """
                    ¡Increíble **%s**! Ahora eres un %s **COLEGIAL** %s :tada::tada::raised_hands_tone3::raised_hands_tone3:

                    ¿Fácil, no?
                    Bueno, ahora sí me presento formalmente.

                    Hola %s **%s** %s,

                    Me alegra mucho que estés aquí en el Colegio.
                    Yo soy el **Smart Assistant** del servidor y formo parte del equipo de **%s**,
                    donde estamos organizados en sub-equipos para ayudarte mejor.

                    Permíteme presentarte al equipo **_%s_**, uno de nuestros sub-equipos más destacados durante esta semana de orientación!.
                    Juntos, nos esforzamos para ofrecerte el mejor soporte y resolver cualquier duda que tengas.
                    Tus estudiantes orientadores de tu equipo son: %s

                    Para que tengas una idea, te puedo ayudar a:

                    > ### :mag_right:Búsqueda de lugares y edificios
                    > - Encontrar edificios
                    > - Encontrar sitios de comer
                    > - Salones de estudio

                    > ### :bulb:Información de contactos
                    > - Oficinas importantes
                    > - Departamentos y facultades
                    > - Administración y servicios

                    > ### :link:Links
                    > - Guía prepística
                    > - Proyectos y organizaciones
                    > - Enlaces para complementar información

                    Espero ser de gran ayuda para tí, recuerda aquí siempre a la orden!!

                    Si quieres, puedes empezar por utilizando los *slash commands*.
                    Para empezar, puedes intentar ``/help`` y veras como te sale un menú donde
                    podrás ver varios de mis comandos que tengo.
                    """,
                member.getFirstname(),
                emojiMention(server, "Huella", "🐾"),
                emojiMention(server, "Huella", "🐾"),
                emojiMention(server, "Huella", "🐾"),
                member.getFirstname(),
                emojiMention(server, "Huella", "🐾"),
                "ECE".equalsIgnoreCase(discordServer.getDepartment()) ? "TEAM-MADE" : "INSO/CIIC",
                team.getName(),
                orientadorNames))
        .queue();
  }

  private void sendWelcomeMessageToOrientador(
      PrivateChannel privateChannel, Guild server, MemberDTO member) {
    // Obtain discord server information
    DiscordServerDTO discordServer = super.getServerOwnerInfo(server.getIdLong());

    privateChannel
        .sendMessage(
            String.format(
                """
                    Bienvenido %s **%s** %s al **%s** Discord Server.
                    Recuerda, avisar a los Bot Developers de cualquier problema con el bot.
                    De tener alguna idea respecto al bot o del server como tal, puedes decirle
                    a los Administradores o a los Bot Developers!!
                    """,
                emojiMention(server, "Huella", "🐾"),
                member.getFirstname(),
                emojiMention(server, "Huella", "🐾"),
                "ECE".equalsIgnoreCase(discordServer.getDepartment()) ? "TEAM-MADE" : "INSO/CIIC"))
        .queue();
  }

  private void applyRoles(
      InteractionHook hook, Guild server, Member member, List<String> roleIds) {
    for (String roleId : roleIds) {
      Role role = server.getRoleById(roleId);
      if (role == null) {
        Logger.instance().logFile(
            LogFeedback.WARNING, "Configured Discord role no longer exists: %s", roleId);
        continue;
      }

      try {
        server
            .addRoleToMember(member, role)
            .queue(
                success -> Logger.instance().logFile(
                    LogFeedback.SUCCESS,
                    "Given Role [%s] to [%s]",
                    role.getName(),
                    member.getEffectiveName()),
                error -> Logger.instance().logFile(
                    LogFeedback.ERROR,
                    "Failed giving Role [%s] to [%s]",
                    role.getName(),
                    member.getEffectiveName()));
      } catch (HierarchyException he) {
        hook.sendMessage("No se pudo otorgar los roles, por favor notifique a un Bot developer")
            .setEphemeral(true)
            .queue();
        Logger.instance().logFile(
            LogFeedback.WARNING,
            "Failed to add role: %s to member %s",
            he.getMessage(),
            member.getEffectiveName());
      }
    }
  }

  private void applyTeam(InteractionHook hook, Guild server, Member member, String email) {
    // Obtain the team that the user has associated to the email
    Optional<TeamDTO> team = service.getMemberTeam(email, server.getIdLong());

    if (team.isEmpty()) {
      hook.sendMessage("No se pudo otorgar el equipo, por favor notifique a un Bot developer")
          .setEphemeral(true)
          .queue();
      Logger.instance()
          .logFile(LogFeedback.ERROR, "Failed to get team role for: %s", member.getEffectiveName());
      return;
    }

    Role role = server.getRoleById(team.get().getTeamRole().getRoleid());

    try {
      server
          .addRoleToMember(member, role)
          .queue(
              success -> Logger.instance()
                  .logFile(
                      LogFeedback.SUCCESS,
                      "Given Role [%s] to [%s]",
                      role.getName(),
                      member.getEffectiveName()),
              error -> Logger.instance()
                  .logFile(
                      LogFeedback.ERROR,
                      "Failed giving Role [%s] to [%s]",
                      role.getName(),
                      member.getEffectiveName()));

      delayInteractions(2000); // two seconds
    } catch (HierarchyException he) {
      hook.sendMessage("No se pudo otorgar los roles, por favor notifique a un Bot developer")
          .setEphemeral(true)
          .queue();
      Logger.instance()
          .logFile(
              LogFeedback.WARNING,
              "Failed to add role: %s to member %s",
              he.getMessage(),
              member.getEffectiveName());
    }
  }

  private void applyNickname(
      InteractionHook hook, Guild server, Member member, MemberDTO memberDTO) {
    String nickname = memberDTO.getFirstname() + " " + memberDTO.getInitial() + " " + memberDTO.getLastname();

    try {
      server
          .modifyNickname(member, nickname)
          .queue(
              success -> Logger.instance()
                  .logFile(
                      LogFeedback.SUCCESS,
                      "Successfully changed nickname from [%s] to [%s]",
                      member.getEffectiveName(),
                      nickname),
              error -> Logger.instance()
                  .logFile(
                      LogFeedback.ERROR,
                      "Failed changing nickname from [%s] to [%s]",
                      member.getEffectiveName(),
                      nickname));

      delayInteractions(2000); // two seconds
    } catch (HierarchyException he) {
      hook.sendMessage("No se pudo otorgar el nickname, por favor notifique a un Bot developer")
          .setEphemeral(true)
          .queue();
      Logger.instance()
          .logFile(LogFeedback.WARNING, "Failed to change nickname: %s", he.getMessage());
    }
  }

  private void delayInteractions(long miliseconds) {
    try {
      Thread.sleep(miliseconds);
    } catch (InterruptedException ie) {
      Logger.instance()
          .logFile(LogFeedback.ERROR, "Thread sleep exception: %s", ie.getLocalizedMessage());
    }
  }
}
