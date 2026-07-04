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

import java.util.List;
import java.util.Optional;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.app.model.MemberPosition;
import assistant.app.embeds.information.RulesEmbed;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.service.GameService;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.entities.Role;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * 
 * @author Alfredo
 *
 */
public class RulesCmd extends InteractionModel implements CommandI {

	private RulesEmbed embed;
	private GameService commandEventService;
	
	private boolean isGlobal;
	
	public RulesCmd() {
		this.embed = new RulesEmbed();
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
		return "rules";
	}

	@Override
	public String getDescription() {
		return "Provee las reglas del servidor";
	}

	@Override
	public List<OptionData> getOptions(Guild server) {
		return List.of();
	}

	@Override
	public void execute(SlashCommandInteractionEvent event) {
		if (event.isFromGuild())
			fromServer(event);
		else
			fromDM(event);
	}
	
	private void fromServer(SlashCommandInteractionEvent event) {
		// Mentioned Roles in embedded message
		Optional<Role> staRole = super.getEffectiveRole(MemberPosition.STAFF, event.getGuild());
		Optional<Role> modRole = super.getEffectiveRole(MemberPosition.MODERATOR, event.getGuild());
		Optional<Role> bdeRole = super.getEffectiveRole(MemberPosition.BOT_DEVELOPER, event.getGuild());
		Optional<Role> conRole = super.getEffectiveRole(MemberPosition.CONSEJERO_PROFESIONAL, event.getGuild());
		Optional<Role> esoRole = super.getEffectiveRole(MemberPosition.ESTUDIANTE_ORIENTADOR, event.getGuild());
		Optional<Role> botRole = super.getEffectiveRole(MemberPosition.BOTS, event.getGuild());
		
		String admRole = event.getGuild().getRolesByName("Administrator", true).stream().findFirst().map(Role::getAsMention).orElse("Administrator");
		
		DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
		int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);
		
		event.replyEmbeds(embed.buildGeneralRules(color, 
				roleMention(esoRole, "Estudiante Orientador"),
				roleMention(botRole, "Bots"),
				roleMention(modRole, "Moderator"),
				roleMention(staRole, "Staff"),
				admRole,
				roleMention(conRole, "Consejero Profesional")),
				
			embed.buildServerUsageRules(color, 
				roleMention(bdeRole, "Bot Developer"),
				roleMention(modRole, "Moderator"),
				roleMention(staRole, "Staff")),
			
			embed.buildBotUsageRules(color, 
				roleMention(bdeRole, "Bot Developer"),
				roleMention(esoRole, "Estudiante Orientador"))).queue();
		
		// Update the user points stats when he uses the command
		commandEventService.updateCommandUserCount(this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
	}
	
	private void fromDM(SlashCommandInteractionEvent event) {
		event.replyEmbeds(embed.buildGeneralRules(0x808080, 
				"Estudiante Orientador",
				"Bots",
				"Moderador",
				"Staff",
				"Administrator",
				"Consejero Profesional"),
				
			embed.buildServerUsageRules(0x808080, 
				"BotDeveloper",
				"Moderador",
				"Staff"),
			
			embed.buildBotUsageRules(0x808080, 
				"BotDeveloper",
				"Estudiante Orientador")).queue();
	}
}
