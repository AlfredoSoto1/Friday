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
import java.util.stream.Collectors;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.app.embeds.information.ProjectsEmbed;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.dto.ProjectDTO;
import assistant.backend.service.GameService;
import assistant.backend.service.ProjectsService;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.Command.Choice;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 *
 */
public class ProjectsCmd extends InteractionModel implements CommandI {

	private static final String COMMAND_LABEL = "select-projects";
	
	private boolean isGlobal;
	
	private ProjectsEmbed embed;
	private ProjectsService service;
	private GameService commandEventService;
	
	public ProjectsCmd() {
		this.embed = new ProjectsEmbed();
		this.service = new ProjectsService();
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
		return "ls_projects";
	}

	@Override
	public String getDescription() {
		return "Información de los proyectos aqui en el RUM para ustedes de INEL/ICOM/INSO/CIIC";
	}

	@Override
	public List<OptionData> getOptions(Guild server) {
		List<String> names = service.getProjectNames(0, 10);
		
		List<Choice> choices = names.stream()
				.map(name -> new Choice(name, name))
				.collect(Collectors.toList());
		
		return List.of(
			new OptionData(OptionType.STRING, COMMAND_LABEL, "Escoje un departamento", true)
				.addChoices(choices));
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
	
	public void fromServer(SlashCommandInteractionEvent event, String selectedProject) {
		DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
		int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);
		
		Optional<ProjectDTO> project = service.getProject(selectedProject);
		
		if (project.isPresent()) {
			event.replyEmbeds(embed.buildProject(color, project.get()))
				.setEphemeral(event.isFromGuild()).queue();
		} else {
			event.reply("Hmm creo que el proyecto que me diste no existe en mi base de datos :confused:")
				.setEphemeral(event.isFromGuild()).queue();
		}

		// Update the user points stats when he uses the command
		commandEventService.updateCommandUserCount(this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
	}

	public void fromDM(SlashCommandInteractionEvent event, String selectedProject) {
		Optional<ProjectDTO> project = service.getProject(selectedProject);
		
		if (project.isPresent()) {
			event.replyEmbeds(embed.buildProject(0x808080, project.get())).queue();
		} else {
			event.reply("Hmm creo que el proyecto que me diste no existe en mi base de datos :confused:").queue();
		}
	}
}
