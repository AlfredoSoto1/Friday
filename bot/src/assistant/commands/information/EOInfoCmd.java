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

import java.awt.Color;
import java.util.List;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.app.model.MemberRetrievement;
import assistant.app.embeds.information.EOEmbed;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.dto.MemberDTO;
import assistant.backend.service.GameService;
import assistant.backend.service.MemberService;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 *
 */
public class EOInfoCmd extends InteractionModel implements CommandI {

	private EOEmbed embed;
	private MemberService service;
	private GameService commandEventService;
	
	public EOInfoCmd() {
		this.embed = new EOEmbed();
		this.service = new MemberService();
		this.commandEventService = new GameService();
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
		return "estudiantes-orientadores";
	}

	@Override
	public String getDescription() {
		return "Get EOs de un programa";
	}

	@Override
	public List<OptionData> getOptions(Guild server) {
		return List.of(
			new OptionData(OptionType.INTEGER, "page", "select the page", true)
				.setRequired(true)
				.setMinValue(0));
	}

	@Override
	public void execute(SlashCommandInteractionEvent event) {
		
		int page = event.getOption("page").getAsInt();
		
		Guild server = event.getGuild();
		DiscordServerDTO discordServer = super.getServerOwnerInfo(server.getIdLong());
		String department = discordServer.getDepartment();
		Color color = Color.decode("#" + discordServer.getColor());
		
		List<MemberDTO> member = service.getAllMembers(page, 3, MemberRetrievement.VERIFIED_ORIENTADOR, server.getIdLong());
		long maxPages = service.memberCount(MemberRetrievement.VERIFIED_ORIENTADOR, server.getIdLong()) / 3;
		
		if (member.isEmpty()) {
			event.reply(String.format(
					"""
					Hmm no creo que hallan demasiados estudiantes ortientadores en esta página,
					trata con un rango de páginas de [0-%s]
					""", maxPages))
				.setEphemeral(true).queue();
			return;
		}
		
		event.replyEmbeds(embed.buildEO(color, department, member, page, maxPages))
			.setEphemeral(true).queue();
		
		// Update the user points stats when he uses the command
		commandEventService.updateCommandUserCount(this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
	}
}
