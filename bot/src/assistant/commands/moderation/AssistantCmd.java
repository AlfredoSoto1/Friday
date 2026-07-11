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

import java.util.List;

import assistant.backend.BackendClient;
import assistant.backend.dto.BotSyncRequest;
import assistant.backend.dto.BotSyncResult;
import assistant.backend.dto.BotSyncRole;
import assistant.app.discord.BotApplication;
import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.app.model.AssistantOptions;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 *
 */
public class AssistantCmd extends InteractionModel implements CommandI {
	
	private static final String COMMAND_LABEL = "service";
	
	private BotApplication bot;
	
	public AssistantCmd(BotApplication bot) {
		this.bot = bot;
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
		return "assistant";
	}

	@Override
	public String getDescription() {
		return "Manage the bot service on server";
	}

	@Override
	public List<OptionData> getOptions(Guild server) {
		return List.of(
			new OptionData(OptionType.STRING, COMMAND_LABEL, "Choose a command", true)
				.addChoice("disable", AssistantOptions.DISABLE.getOption())
				.addChoice("sync", AssistantOptions.SYNC.getOption())
			);
	}

	@Override
	public void execute(SlashCommandInteractionEvent event) {
		if(!super.validateCommandUse(event))
			return;
		
		AssistantOptions option = AssistantOptions.asOption(event.getOption(COMMAND_LABEL).getAsString());
		
		switch(option) {
		case DISABLE:
			event.reply("Shutting down...").setEphemeral(event.isFromGuild()).queue();
			bot.shutdown();
			break;
		case SYNC:
			sync(event);
			break;
		default:
			// skip this action if no reply was provided
			event.reply("Mmhh this command does nothing, try again with another one")
				.setEphemeral(event.isFromGuild()).queue();
		}
	}

	private void sync(SlashCommandInteractionEvent event) {
		Guild guild = event.getGuild();
		List<BotSyncRole> roles = guild.getRoles().stream()
			.map(role -> new BotSyncRole(
				role.getId(),
				role.getName(),
				role.getColorRaw(),
				role.getPosition(),
				role.isManaged(),
				role.isMentionable(),
				role.isHoisted()))
			.toList();
		event.deferReply(true).queue(hook -> {
			BotSyncResult result = BackendClient.syncGuild(new BotSyncRequest(
				guild.getIdLong(),
				guild.getName(),
				roles));

			String message = !result.serverRegistered()
				? "This server is not registered in Friday."
				: result.syncedAt() == null
					? "Unable to synchronize this server with the backend."
					: String.format("Synchronized %d roles.", result.roleCount());
			hook.editOriginal(message).queue();
		});
	}
}
