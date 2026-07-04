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
package assistant.app.interactions;

import java.util.List;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.build.CommandData;
import net.dv8tion.jda.api.interactions.commands.build.Commands;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

/**
 * @author Alfredo
 */
public interface CommandI {

  /**
   * @return true if the command is global
   */
  public boolean isGlobal();

  /**
   * Changes the command status of being attached to a guild to now being global this means, that
   * you can DM the bot and use such command.
   *
   * @param isGlobal
   */
  public void setGlobal(boolean isGlobal);

  /**
   * @return name of the command
   */
  public String getCommandName();

  /**
   * @return description of the command
   */
  public String getDescription();

  /**
   * @return the options that the command has
   */
  public List<OptionData> getOptions(Guild server);

  /**
   * Executes the command
   *
   * @param event
   */
  public void execute(SlashCommandInteractionEvent event);

  default CommandData commandData(Guild guild) {
    return Commands.slash(getCommandName(), getDescription()).addOptions(getOptions(guild));
  }
}
