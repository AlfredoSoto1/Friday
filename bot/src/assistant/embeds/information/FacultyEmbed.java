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
package assistant.embeds.information;

import assistant.backend.dto.FacultyDTO;
import assistant.backend.dto.WebpageDTO;
import assistant.backend.dto.ContactDTO;
import assistant.embeds.EmbedValues;
import java.util.Collections;
import java.util.List;
import java.util.stream.Collectors;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

/**
 * @author Alfredo
 */
public class FacultyEmbed {

  public FacultyEmbed() {}

  public MessageEmbed buildFaculty(
      int color, String department, List<FacultyDTO> faculty, long page, long maxPages) {

    EmbedBuilder embed =
        new EmbedBuilder().setColor(color).setTitle(EmbedValues.na(department) + " faculty").setDescription("");

    for (FacultyDTO professor : faculty) {
      ContactDTO contact = professor.getContact();
      List<String> webPages =
          contact == null || contact.getWebpages() == null
              ? Collections.emptyList()
              : contact.getWebpages().stream()
              .map(webpage -> webpage == null ? "N/A" : EmbedValues.na(webpage.getUrl()))
              .collect(Collectors.toList());
      String extensions = contact == null || contact.getExtensions() == null
          ? "N/A"
          : contact.getExtensions().isEmpty()
              ? "N/A"
              : contact.getExtensions().stream()
                  .map(extension -> extension == null ? "N/A" : EmbedValues.na(extension.getExt()))
                  .collect(Collectors.joining(", "));

      embed.addField(
          EmbedValues.na(professor.getName()),
          String.format(
              """
              > %s
              > \u200B
              > - **%s**
              > - _%s_
              > - Office: _%s_
              > - Ext. %s
              %s
              """,
              EmbedValues.na(professor.getDescription()),
              EmbedValues.na(professor.getJobentitlement()),
              contact == null ? "N/A" : EmbedValues.na(contact.getEmail()),
              EmbedValues.na(professor.getOffice()),
              extensions,
              webPages.isEmpty() ? "> - Página oficial: N/A" : "> - Página oficial: " + String.join(", ", webPages)),
          false);
    }

    embed.addField(
        "",
        String.format(
            """
            %s of %s
            Para más contactos de Facultad
            %s
            """,
            page,
            maxPages,
            EmbedValues.na(department).equalsIgnoreCase("ECE")
                ? "https://ece.uprm.edu/people/faculty/#cn-top"
                : "https://www.uprm.edu/cse/faculty/"),
        false);

    return embed.build();
  }
}
