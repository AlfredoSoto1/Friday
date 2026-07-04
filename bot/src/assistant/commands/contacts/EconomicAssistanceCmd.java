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

import java.util.List;
import java.util.Optional;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.app.embeds.contacts.ServicesEmbed;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.dto.ServiceDTO;
import assistant.backend.service.GameService;
import assistant.backend.service.ServicesService;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 *
 */
public class EconomicAssistanceCmd extends InteractionModel implements CommandI {
	
	private ServicesEmbed embed;
	private ServicesService service;
	private GameService commandEventService;

	private boolean isGlobal;
	
	public EconomicAssistanceCmd() {
		this.embed = new ServicesEmbed();
		this.service = new ServicesService();
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
		return "contact-asistencia-economica";
	}

	@Override
	public String getDescription() {
		return "Información acerca de la oficina de asistencia economica";
	}

	@Override
	public List<OptionData> getOptions(Guild server) {
		return List.of();
	}

	@Override
	public void execute(SlashCommandInteractionEvent event) {
		// Obtain the service data related to the Asistencia Económica
		Optional<ServiceDTO> result = service.getService("Asistencia Económica");
		
		// Check if the command was called from a server
		if (event.isFromGuild()) {
			DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
			int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);
			
			event.replyEmbeds(embed.buildInfoPanel(color, result.get()))
				.setEphemeral(event.isFromGuild()).queue();
			
			// Update the user points stats when he uses the command
			commandEventService.updateCommandUserCount(this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
		} else {
			event.replyEmbeds(embed.buildInfoPanel(0x808080, result.get())).queue();
		}
	}
}
