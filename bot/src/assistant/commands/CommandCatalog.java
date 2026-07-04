package assistant.commands;

import assistant.app.embeds.EmbedFactory;
import assistant.app.interactions.InteractionDefinition;
import assistant.app.interactions.InteractionRegistry;

import java.util.ArrayList;
import java.util.List;

public final class CommandCatalog {
  private CommandCatalog() {
  }

  public static InteractionRegistry createDefault(EmbedFactory embedFactory) {
    List<InteractionDefinition> interactions = new ArrayList<>();

    interactions.add(content("calendario", "Links to the academic calendar of UPRM.", embedFactory));
    interactions.add(content("estudiante-orientador", "Lists all EO's in the server by department.", embedFactory));
    interactions.add(content("rules", "Displays the rules and guidelines for the Discord server.", embedFactory));
    interactions.add(content("guia-prepistica", "Provides a guide for incoming freshmen.", embedFactory));
    interactions.add(content("faculty", "Displays detailed information about faculty members.", embedFactory));
    interactions.add(content("ls_projects", "Provides information on ongoing projects and research.", embedFactory));
    interactions.add(content("ls_student_orgs", "Lists student organizations and their activities.", embedFactory));
    interactions.add(content("salon", "Finds a building on campus based on its code.", embedFactory));
    interactions.add(content("lab", "Finds a lab on campus based on its code.", embedFactory));
    interactions.add(content("links", "Provides useful links related to studies and resources.", embedFactory));
    interactions.add(content("made-web", "Links to the MADE website.", embedFactory));
    interactions.add(content("contact-dcsp", "Provides information about the Department of Computer Science and Engineering.", embedFactory));
    interactions.add(content("contact-department", "Displays information about specific departments at UPRM.", embedFactory));
    interactions.add(content("contact-decanato-estudiantes", "Provides contact information for the Dean of Students.", embedFactory));
    interactions.add(content("contact-guardia-univ", "Provides contact information for campus security.", embedFactory));
    interactions.add(content("contact-asesoria-academica", "Provides guidance on academic matters and advisories.", embedFactory));
    interactions.add(content("contact-asistencia-economica", "Provides information about financial aid and economic assistance.", embedFactory));
    interactions.add(content("curriculo", "Provides general information about academic curricula.", embedFactory));
    interactions.add(content("faq", "Answers frequently asked questions about the Discord server.", embedFactory));
    interactions.add(content("help", "Displays a comprehensive help menu for navigating the bot's commands.", embedFactory));
    interactions.add(content("map", "Provides a link to the UPRM campus map.", embedFactory));

    interactions.add(new SyncCommand());
    interactions.add(new assistant.commands.games.CoinFlipCommand(embedFactory));
    interactions.add(new assistant.interactions.games.LevelingInteraction(embedFactory));

    return new InteractionRegistry(interactions);
  }

  private static BackendContentCommand content(
    String name,
    String description,
    EmbedFactory embedFactory) {
    return new BackendContentCommand(name, description, embedFactory);
  }
}
