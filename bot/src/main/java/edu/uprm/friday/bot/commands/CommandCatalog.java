package edu.uprm.friday.bot.commands;

import edu.uprm.friday.bot.backend.BackendClient;
import edu.uprm.friday.bot.embeds.EmbedFactory;
import edu.uprm.friday.bot.interactions.InteractionDefinition;
import edu.uprm.friday.bot.interactions.InteractionRegistry;

import java.util.ArrayList;
import java.util.List;

public final class CommandCatalog {
  private CommandCatalog() {
  }

  public static InteractionRegistry createDefault(BackendClient backendClient, EmbedFactory embedFactory) {
    List<InteractionDefinition> interactions = new ArrayList<>();

    interactions.add(content("calendario", "Links to the academic calendar of UPRM.", backendClient, embedFactory));
    interactions.add(content("estudiante-orientador", "Lists all EO's in the server by department.", backendClient, embedFactory));
    interactions.add(content("rules", "Displays the rules and guidelines for the Discord server.", backendClient, embedFactory));
    interactions.add(content("guia-prepistica", "Provides a guide for incoming freshmen.", backendClient, embedFactory));
    interactions.add(content("faculty", "Displays detailed information about faculty members.", backendClient, embedFactory));
    interactions.add(content("ls_projects", "Provides information on ongoing projects and research.", backendClient, embedFactory));
    interactions.add(content("ls_student_orgs", "Lists student organizations and their activities.", backendClient, embedFactory));
    interactions.add(content("salon", "Finds a building on campus based on its code.", backendClient, embedFactory));
    interactions.add(content("lab", "Finds a lab on campus based on its code.", backendClient, embedFactory));
    interactions.add(content("links", "Provides useful links related to studies and resources.", backendClient, embedFactory));
    interactions.add(content("made-web", "Links to the MADE website.", backendClient, embedFactory));
    interactions.add(content("contact-dcsp", "Provides information about the Department of Computer Science and Engineering.", backendClient, embedFactory));
    interactions.add(content("contact-department", "Displays information about specific departments at UPRM.", backendClient, embedFactory));
    interactions.add(content("contact-decanato-estudiantes", "Provides contact information for the Dean of Students.", backendClient, embedFactory));
    interactions.add(content("contact-guardia-univ", "Provides contact information for campus security.", backendClient, embedFactory));
    interactions.add(content("contact-asesoria-academica", "Provides guidance on academic matters and advisories.", backendClient, embedFactory));
    interactions.add(content("contact-asistencia-economica", "Provides information about financial aid and economic assistance.", backendClient, embedFactory));
    interactions.add(content("curriculo", "Provides general information about academic curricula.", backendClient, embedFactory));
    interactions.add(content("faq", "Answers frequently asked questions about the Discord server.", backendClient, embedFactory));
    interactions.add(content("help", "Displays a comprehensive help menu for navigating the bot's commands.", backendClient, embedFactory));
    interactions.add(content("map", "Provides a link to the UPRM campus map.", backendClient, embedFactory));

    interactions.add(new VerificationInteraction(backendClient, embedFactory));
    interactions.add(new SyncCommand(backendClient));
    interactions.add(new CoinFlipCommand(backendClient, embedFactory));
    interactions.add(new LevelingInteraction(backendClient, embedFactory));

    return new InteractionRegistry(interactions);
  }

  private static BackendContentCommand content(
    String name,
    String description,
    BackendClient backendClient,
    EmbedFactory embedFactory) {
    return new BackendContentCommand(name, description, backendClient, embedFactory);
  }
}
