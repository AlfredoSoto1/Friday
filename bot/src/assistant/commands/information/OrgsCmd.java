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
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.dto.OrganizationDTO;
import assistant.backend.service.GameService;
import assistant.backend.service.OrganizationsService;
import assistant.embeds.information.OrganizationsEmbed;
import java.util.List;
import java.util.Optional;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 */
public class OrgsCmd extends InteractionModel implements CommandI {

  private static final String COMMAND_LABEL = "select-orgs";

  private OrganizationsEmbed embed;
  private OrganizationsService service;
  private GameService commandEventService;

  private boolean isGlobal;

  public OrgsCmd() {
    this.embed = new OrganizationsEmbed();
    this.service = new OrganizationsService();
    this.commandEventService = new GameService();
  }

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
    return "ls_organizations";
  }

  @Override
  public String getDescription() {
    return "Obten una lista te todas las organizaciones para tí";
  }

  @Override
  public List<OptionData> getOptions(Guild server) {
    OptionData organizations =
        new OptionData(OptionType.STRING, COMMAND_LABEL, "Selecciona la organización", true);
    service.getOrganizationNames(0, 25).forEach(name -> organizations.addChoice(name, name));
    return List.of(organizations);
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    String selectedProject = event.getOption(COMMAND_LABEL).getAsString();
    if (event.isFromGuild()) {
      fromServer(event, selectedProject);
    } else {
      fromDM(event, selectedProject);
    }
  }

  private void fromServer(SlashCommandInteractionEvent event, String selectedProject) {
    DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
    int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);

    Optional<OrganizationDTO> project = service.getOrganization(selectedProject);

    if (project.isPresent()) {
      event
          .replyEmbeds(embed.buildOrganization(color, project.get()))
          .setEphemeral(event.isFromGuild())
          .queue();
    } else {
      event
          .reply(
              "Hmm creo que la organización que me diste no existe en mi base de datos :confused:")
          .setEphemeral(event.isFromGuild())
          .queue();
    }

    // Update the user points stats when he uses the command
    commandEventService.updateCommandUserCount(
        this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
  }

  private void fromDM(SlashCommandInteractionEvent event, String selectedProject) {
    Optional<OrganizationDTO> project = service.getOrganization(selectedProject);

    if (project.isPresent()) {
      event.replyEmbeds(embed.buildOrganization(0x808080, project.get())).queue();
    } else {
      event
          .reply(
              "Hmm creo que la organización que me diste no existe en mi base de datos :confused:")
          .queue();
    }
  }
}
