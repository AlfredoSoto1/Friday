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
import assistant.backend.BackendClient;
import assistant.backend.dto.BotGuildProfile;
import assistant.backend.service.GameService;
import java.util.List;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;
import net.dv8tion.jda.api.interactions.components.buttons.Button;

/**
 * @author Alfredo
 */
public class CurriculumCmd extends InteractionModel implements CommandI {

  private static final String COMMAND_LABEL = "program";

  private static final String OPTION_CHOICE_INEL = "INEL";
  private static final String OPTION_CHOICE_ICOM = "ICOM";
  private static final String OPTION_CHOICE_INSO = "INSO";
  private static final String OPTION_CHOICE_CIIC = "CIIC";
  private static final String ICOM_CURRICULUM_URL =
      "https://ece.uprm.edu/wp-content/uploads/Curriculo-NUEVO-ICOM-2025-2026.pdf";
  private static final String INEL_CURRICULUM_URL =
      "https://ece.uprm.edu/wp-content/uploads/INEL.pdf";

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
    OptionData programOption =
        new OptionData(OptionType.STRING, COMMAND_LABEL, "Escoje un programa de estudio", true);

    for (String program : allowedPrograms(server)) {
      programOption.addChoice(programLabel(program), program);
    }

    return List.of(programOption);
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    String selectedProgram = event.getOption(COMMAND_LABEL).getAsString();
    List<String> allowedPrograms = allowedPrograms(event.getGuild());
    String curriculumUrl = allowedPrograms.contains(selectedProgram) ? curriculumUrl(selectedProgram) : "";

    if (curriculumUrl.isBlank()) {
      event
          .reply("No curriculum PDF is available for " + selectedProgram + " right now.")
          .setEphemeral(true)
          .queue();
    } else {
      event
          .reply("> Here is the " + selectedProgram + " curriculum:")
          .setActionRow(Button.link(curriculumUrl, "Open PDF"))
          .setEphemeral(true)
          .queue();
    }

    // Update the user points stats when he uses the command
    commandEventService.updateCommandUserCount(
        this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
  }

  private List<String> allowedPrograms(Guild server) {
    if (server == null) {
      return List.of();
    }

    BotGuildProfile profile = BackendClient.guildProfile(server.getIdLong());
    String departmentProfile = profile.departmentProfile();
    if ("INEL_ICOM".equals(departmentProfile)) {
      return List.of(OPTION_CHOICE_INEL, OPTION_CHOICE_ICOM);
    }
    if ("INSO_CIIC".equals(departmentProfile)) {
      return List.of(OPTION_CHOICE_INSO, OPTION_CHOICE_CIIC);
    }
    return List.of();
  }

  private String programLabel(String program) {
    return switch (program) {
      case OPTION_CHOICE_INEL -> "INEL - Electrical Engineering";
      case OPTION_CHOICE_ICOM -> "ICOM - Computer Engineering";
      case OPTION_CHOICE_INSO -> "INSO - Software Engineering";
      case OPTION_CHOICE_CIIC -> "CIIC - Computer Science & Engineering";
      default -> program;
    };
  }

  private String curriculumUrl(String program) {
    return switch (program) {
      case OPTION_CHOICE_INEL -> INEL_CURRICULUM_URL;
      case OPTION_CHOICE_ICOM -> ICOM_CURRICULUM_URL;
      default -> "";
    };
  }
}
