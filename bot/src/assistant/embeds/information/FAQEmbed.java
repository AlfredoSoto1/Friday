package assistant.embeds.information;

import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;
import net.dv8tion.jda.api.entities.MessageEmbed.Field;

public final class FAQEmbed {
  private static final int FIELDS_PER_PAGE = 4;

  public MessageEmbed buildFAQ(
      int color, String developerRole, String advisorRole, int page) {
    return build(color, null, developerRole, advisorRole, page);
  }

  public MessageEmbed buildFAQDM(int color, String developerRole, String advisorRole, int page) {
    return build(color, null, developerRole, advisorRole, page);
  }

  private MessageEmbed build(
      int color, String banner, String developerRole, String advisorRole, int page) {
    List<Field> questions = questions(developerRole, advisorRole);
    int totalPages = Math.max(1, (questions.size() + FIELDS_PER_PAGE - 1) / FIELDS_PER_PAGE);

    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("❓ Frequently Asked Questions")
            .setDescription(
                "Respuestas rápidas para navegar el servidor y comenzar tu vida universitaria.")
            .setFooter(
                "Página "
                    + (Math.min(page, totalPages - 1) + 1)
                    + " de "
                    + totalPages
                    + "  •  Usa /help para ver comandos");

    if (page < 0 || page >= totalPages) {
      return embed
          .setDescription(
              "No existe esa página. Selecciona una página entre `0` y `" + (totalPages - 1) + "`.")
          .build();
    }

    int start = page * FIELDS_PER_PAGE;
    questions.stream().skip(start).limit(FIELDS_PER_PAGE).forEach(embed::addField);
    return embed.build();
  }

  private List<Field> questions(String developerRole, String advisorRole) {
    return List.of(
        question(
            "01",
            "¿Cómo aprendo a usar Discord?",
            "Consulta `/help` o comunícate con " + developerRole + "."),
        question(
            "02",
            "¿Por qué no veo todos los canales?",
            "Los canales se organizan por grupos para mantener conversaciones claras y"
                + " relevantes."),
        question(
            "03",
            "Tengo una idea para el bot",
            "Compártela con " + developerRole + " o " + advisorRole + "."),
        question(
            "04",
            "¿Dónde encuentro los comandos?",
            "Ejecuta `/help` para ver el catálogo actualizado."),
        question(
            "05",
            "¿Cómo se creó Friday?",
            "Friday está desarrollado en Java con JDA. "
                + developerRole
                + " puede orientarte si deseas colaborar."),
        question(
            "06",
            "Tengo dudas sobre mi departamento",
            "Habla con tu asesor académico, líder de grupo o consejero departamental."),
        question(
            "07",
            "Necesito convalidar cursos",
            "Sigue la orientación de convalidaciones y confirma el proceso con tu departamento"
                + " durante ajustes."),
        question(
            "08",
            "¿Dónde consigo mi currículo?",
            "Usa `/curriculo` y selecciona tu programa académico."),
        question(
            "09",
            "¿Qué significan D, E y H en una sección?",
            "`D` remota asincrónica  •  `E` remota con horario  •  `H` híbrida"),
        question(
            "10",
            "¿Cuántos créditos necesito?",
            "La carga regular comienza en 12 créditos. Confirma excepciones con tu asesor."),
        question(
            "11",
            "¿Qué es Moodle?",
            "Es la plataforma institucional de cursos: [online.upr.edu](https://online.upr.edu)."),
        question(
            "12",
            "¿Dónde puedo estacionarme?",
            "Consulta las áreas autorizadas y la reglamentación vigente de tránsito del recinto."),
        question(
            "13",
            "¿A quién contacto si sigo teniendo dudas?",
            "Comunícate con " + advisorRole + " o utiliza los comandos de contacto en `/help`."));
  }

  private Field question(String number, String title, String answer) {
    return new Field("`" + number + "`  " + title, answer, false);
  }
}
