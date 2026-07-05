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
package assistant.commands.links;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.service.GameService;
import assistant.embeds.information.LinksEmbed;
import java.util.List;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 */
public class LinksCmd extends InteractionModel implements CommandI {

  private LinksEmbed embed;
  private GameService commandEventService;

  private boolean isGlobal;

  public LinksCmd() {
    this.embed = new LinksEmbed();
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
    return "links";
  }

  @Override
  public String getDescription() {
    return "Una lista de links importantes";
  }

  @Override
  public List<OptionData> getOptions(Guild server) {
    return List.of();
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    if (event.isFromGuild()) fromServer(event);
    else fromDM(event);
  }

  private void fromServer(SlashCommandInteractionEvent event) {
    DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
    int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);

    event.replyEmbeds(embed.buildLinks(color)).setEphemeral(true).queue();

    // Update the user points stats when he uses the command
    commandEventService.updateCommandUserCount(
        this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
  }

  private void fromDM(SlashCommandInteractionEvent event) {
    event.replyEmbeds(embed.buildLinks(0x808080)).queue();
  }
}
