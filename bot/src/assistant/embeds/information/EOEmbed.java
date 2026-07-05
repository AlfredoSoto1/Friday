package assistant.embeds.information;

import assistant.backend.dto.MemberDTO;
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
                    + safe(department, "tu departamento")
                    + "**. Están aquí para ayudarte a comenzar con confianza.")
            .setFooter("Página " + (page + 1) + " de " + Math.max(totalPages, 1));

    for (MemberDTO member : members) {
      String name = (safe(member.getFirstname(), "") + " " + safe(member.getLastname(), "")).trim();
      String program =
          member.getProgram() == null
              ? "Programa no especificado"
              : member.getProgram().getLiteral();
      String fact =
          safe(member.getFunfact(), "Disponible para orientarte durante tu transición al Recinto.");
      embed.addField(
          "👤  " + safe(name, "Estudiante Orientador"),
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

  private String safe(String value, String fallback) {
    return value == null || value.isBlank() ? fallback : value.trim();
  }
}
