package assistant.embeds.information;

import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;
import net.dv8tion.jda.api.entities.MessageEmbed.Field;

public final class HelpEmbed {
  private static final int FIELDS_PER_PAGE = 6;

  public MessageEmbed buildHelp(int color, String advisorRole, int page) {
    return build(color, null, advisorRole, page);
  }

  public MessageEmbed buildHelpDM(String advisorRole, int page) {
    return build(0x5865F2, null, advisorRole, page);
  }

  private MessageEmbed build(int color, String banner, String advisorRole, int page) {
    List<Field> commands = commands(advisorRole);
    int totalPages = Math.max(1, (commands.size() + FIELDS_PER_PAGE - 1) / FIELDS_PER_PAGE);

    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("✨ Friday Command Center")
            .setDescription(
                "Todo lo que necesitas, organizado por función. Los comandos marcados con"
                    + " `Servidor` requieren un servidor de Discord.")
            .setFooter(
                "Página "
                    + (Math.min(page, totalPages - 1) + 1)
                    + " de "
                    + totalPages
                    + "  •  /faq para preguntas frecuentes");

    if (page < 0 || page >= totalPages) {
      return embed
          .setDescription(
              "No existe esa página. Selecciona una página entre `0` y `" + (totalPages - 1) + "`.")
          .build();
    }

    commands.stream()
        .skip((long) page * FIELDS_PER_PAGE)
        .limit(FIELDS_PER_PAGE)
        .forEach(embed::addField);
    return embed.build();
  }

  private List<Field> commands(String advisorRole) {
    return List.of(
        command("🧭", "/help", "Abre este centro de comandos.", false),
        command("❓", "/faq", "Respuestas a preguntas frecuentes.", false),
        command("📜", "/rules", "Reglas y buenas prácticas del servidor.", false),
        command("🗺️", "/map", "Mapa y recursos de ubicación del recinto.", false),
        command("🔗", "/links", "Enlaces académicos importantes.", false),
        command("📅", "/calendario", "Calendario académico oficial.", false),
        command("🏫", "/salon", "Localiza el edificio de un salón.", false),
        command("🖥️", "/lab", "Busca laboratorios y espacios de estudio.", false),
        command("📄", "/curriculo", "Descarga el currículo de tu programa.", true),
        command("🚀", "/ls_projects", "Explora proyectos estudiantiles y de investigación.", false),
        command("🌐", "/ls_student_orgs", "Descubre organizaciones estudiantiles.", false),
        command("🧑‍🤝‍🧑", "/estudiantes-orientadores", "Conoce a " + advisorRole + ".", true),
        command("🏆", "/rank", "Consulta tu nivel o el leaderboard.", true),
        command("🪙", "/game-coinflip", "Lanza una moneda.", false),
        command("🏓", "/game-ping", "Consulta la latencia del bot.", false),
        command("🎲", "/game-dice", "Lanza un dado configurable.", false),
        command("⚔️", "/game-rps", "Juega piedra, papel o tijera.", false),
        command("🎱", "/game-eightball", "Hazle una pregunta a la bola mágica.", false),
        command(
            "☎️",
            "Contactos",
            "Usa los comandos `contact-*` para oficinas y servicios universitarios.",
            false));
  }

  private Field command(String icon, String name, String description, boolean serverOnly) {
    String scope = serverOnly ? "\n`Servidor`" : "";
    return new Field(icon + "  **" + name + "**", description + scope, true);
  }
}
