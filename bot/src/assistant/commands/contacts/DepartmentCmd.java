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
package assistant.commands.contacts;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.dto.ServiceDTO;
import assistant.backend.service.GameService;
import assistant.backend.service.ServicesService;
import assistant.embeds.contacts.ServicesEmbed;
import java.util.List;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 */
public class DepartmentCmd extends InteractionModel implements CommandI {

  private ServicesEmbed embed;
  private ServicesService service;
  private GameService commandEventService;

  public DepartmentCmd() {
    this.embed = new ServicesEmbed();
    this.service = new ServicesService();
    this.commandEventService = new GameService();
  }

  @Override
  public boolean isGlobal() {
    return false;
  }

  @Override
  @Deprecated
  public void setGlobal(boolean isGlobal) {
    // Server only command
  }

  @Override
  public String getCommandName() {
    return "contact-department";
  }

  @Override
  public String getDescription() {
    return "Información acerca de los departamentos";
  }

  @Override
  public List<OptionData> getOptions(Guild server) {
    return List.of();
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
    String department = discordServer.getDepartment();
    int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);

    if ("ECE".equalsIgnoreCase(department)) {
      ServiceDTO result = service.getService("Electrical and Computer Engineering", null);

      event.replyEmbeds(embed.buildInfoPanel(color, result)).setEphemeral(true).queue();
    } else {
      ServiceDTO result = service.getService("Computer Science & Engineering", null);

      event.replyEmbeds(embed.buildInfoPanel(color, result)).setEphemeral(true).queue();
    }

    // Update the user points stats when he uses the command
    commandEventService.updateCommandUserCount(
        this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
  }
}
