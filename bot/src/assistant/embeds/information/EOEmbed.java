package assistant.embeds.information;

import assistant.backend.dto.MemberDTO;
import assistant.embeds.EmbedValues;
import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class EOEmbed {
  public MessageEmbed buildEO(
      int color, String department, List<MemberDTO> members, long page, long totalPages) {
    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("🤝 Estudiantes Orientadores")
            .setDescription(
                "Conoce al equipo de **"
                    + EmbedValues.na(department)
                    + "**. Están aquí para ayudarte a comenzar con confianza.")
            .setFooter("Página " + (page + 1) + " de " + Math.max(totalPages, 1));

    for (MemberDTO member : members) {
      String name = (EmbedValues.na(member.getFirstname()) + " " + EmbedValues.na(member.getLastname())).trim();
      String program =
          member.getProgram() == null
              ? "N/A"
              : EmbedValues.na(member.getProgram().getLiteral());
      String fact = EmbedValues.na(member.getFunfact());
      embed.addField(
          "👤  " + EmbedValues.na(name),
          "**Programa:** " + program + "\n> " + fact,
          false);
    }
    return embed.build();
  }

  public MessageEmbed buildEmpty(int color, long totalPages) {
    return new EmbedBuilder()
        .setColor(color)
        .setTitle("🧭 Página no disponible")
        .setDescription(
            totalPages == 0
                ? "Todavía no hay estudiantes orientadores publicados."
                : "Selecciona una página entre `0` y `" + (totalPages - 1) + "`.")
        .build();
  }

}
