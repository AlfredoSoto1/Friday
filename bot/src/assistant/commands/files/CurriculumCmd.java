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
package assistant.commands.files;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.backend.service.GameService;
import java.io.File;
import java.util.List;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;
import net.dv8tion.jda.api.utils.FileUpload;

/**
 * @author Alfredo
 */
public class CurriculumCmd extends InteractionModel implements CommandI {

  private static final String COMMAND_LABEL = "program";

  private static final String OPTION_CHOICE_INEL = "INEL";
  private static final String OPTION_CHOICE_ICOM = "ICOM";
  private static final String OPTION_CHOICE_INSO = "INSO";
  private static final String OPTION_CHOICE_CIIC = "CIIC";

  private GameService commandEventService;

  public CurriculumCmd() {
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
    return "curriculo";
  }

  @Override
  public String getDescription() {
    return "PDF del currículo del Programa de Estudio";
  }

  @Override
  public List<OptionData> getOptions(Guild server) {
    return List.of(
        new OptionData(OptionType.STRING, COMMAND_LABEL, "Escoje un programa de estudio", true)
            .addChoice("INEL - Electrical Engineering", OPTION_CHOICE_INEL)
            .addChoice("ICOM - Computer Engineering", OPTION_CHOICE_ICOM)
            .addChoice("INSO - Software Engineering", OPTION_CHOICE_INSO)
            .addChoice("CIIC - Computer Science & Engineering", OPTION_CHOICE_CIIC));
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {

    switch (event.getOption(COMMAND_LABEL).getAsString()) {
      case OPTION_CHOICE_INEL:
        File INELcurriculum = getAsset("curriculos/INEL.pdf");
        event
            .reply("> Here is the Electrical Engineering Curriculum")
            .addFiles(FileUpload.fromData(INELcurriculum))
            .setEphemeral(true)
            .queue();
        break;
      case OPTION_CHOICE_ICOM:
        File ICOMcurriculum = getAsset("curriculos/ICOM.pdf");
        event
            .reply("> Here is the Computer Engineering Curriculum")
            .addFiles(FileUpload.fromData(ICOMcurriculum))
            .setEphemeral(true)
            .queue();
        break;
      case OPTION_CHOICE_INSO:
        File INSOcurriculum = getAsset("curriculos/INSO.pdf");
        event
            .reply("> Here is the Software Engineering Curriculum")
            .addFiles(FileUpload.fromData(INSOcurriculum))
            .setEphemeral(true)
            .queue();
        break;
      case OPTION_CHOICE_CIIC:
        File CIICcurriculum = getAsset("curriculos/CIIC.pdf");
        event
            .reply("> Here is the Computer Science & Engineering Curriculum")
            .addFiles(FileUpload.fromData(CIICcurriculum))
            .setEphemeral(true)
            .queue();
        break;
    }

    // Update the user points stats when he uses the command
    commandEventService.updateCommandUserCount(
        this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
  }
}
