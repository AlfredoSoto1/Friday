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
package assistant.commands.information;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.app.model.MemberPosition;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.service.GameService;
import assistant.embeds.information.FAQEmbed;
import java.util.List;
import java.util.Optional;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.entities.Role;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 */
public class FAQCmd extends InteractionModel implements CommandI {

  private FAQEmbed embed;

  private GameService commandEventService;

  private boolean isGlobal;

  public FAQCmd() {
    this.embed = new FAQEmbed();
    this.commandEventService = new GameService();
  }

  @Override
  public void onGuildInit(Guild server) {}

  @Override
  public boolean isGlobal() {
    return isGlobal;
  }

  @Override
  public void setGlobal(boolean isGlobal) {
    this.isGlobal = isGlobal;
  }

  @Override
  public String getCommandName() {
    return "faq";
  }

  @Override
  public String getDescription() {
    return "Frequently Asked Questions por Prepas";
  }

  @Override
  public List<OptionData> getOptions(Guild server) {
    return List.of(
        new OptionData(OptionType.INTEGER, "page", "Enter page of FAQ", true).setMinValue(0));
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    if (event.isFromGuild()) {
      fromServer(event);
    } else {
      fromDM(event);
    }
  }

  private void fromServer(SlashCommandInteractionEvent event) {
    int page = event.getOption("page").getAsInt();

    // Mentioned Roles in embedded message
    Optional<Role> bdeRole = super.getEffectiveRole(MemberPosition.BOT_DEVELOPER, event.getGuild());
    Optional<Role> esoRole =
        super.getEffectiveRole(MemberPosition.ESTUDIANTE_ORIENTADOR, event.getGuild());

    DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
    String department = discordServer.getDepartment();
    int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);

    if ("ECE".equalsIgnoreCase(department)) {
      event.replyEmbeds(embed.buildFAQ(color, roleMention(bdeRole, "Bot Developer"), roleMention(esoRole, "Estudiante Orientador"), page)).queue();
    } else {
      event.replyEmbeds(embed.buildFAQ(color, roleMention(bdeRole, "Bot Developer"), roleMention(esoRole, "Estudiante Orientador"), page)).queue();
    }

    // Update the user points stats when he uses the command
    commandEventService.updateCommandUserCount(
        this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
  }

  private void fromDM(SlashCommandInteractionEvent event) {
    int page = event.getOption("page").getAsInt();
    event
        .replyEmbeds(embed.buildFAQDM(0x808080, "BotDeveloper", "EstudianteOrientador", page))
        .queue();
  }
}
