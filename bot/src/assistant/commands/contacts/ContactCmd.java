package assistant.commands.contacts;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.backend.dto.DiscordServerDTO;
import assistant.backend.dto.ServiceDTO;
import assistant.backend.service.GameService;
import assistant.backend.service.ServicesService;
import assistant.embeds.contacts.ServicesEmbed;
import java.util.List;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.interaction.command.SlashCommandInteractionEvent;
import net.dv8tion.jda.api.interactions.commands.OptionType;
import net.dv8tion.jda.api.interactions.commands.Command;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

public final class ContactCmd extends InteractionModel implements CommandI {
  private static final String CONTACT_OPTION = "contact";

  private final ServicesEmbed embed = new ServicesEmbed();
  private final ServicesService service = new ServicesService();
  private final GameService commandEventService = new GameService();
  private boolean isGlobal;

  @Override
  public boolean isGlobal() {
    return isGlobal;
  }

  @Override
  public void setGlobal(boolean isGlobal) {
    this.isGlobal = isGlobal;
  }

  @Override
  public String getCommandName() {
    return "contact";
  }

  @Override
  public String getDescription() {
    return "Información de oficinas y servicios universitarios";
  }

  @Override
  public List<OptionData> getOptions(Guild server) {
    OptionData contacts =
        new OptionData(OptionType.INTEGER, CONTACT_OPTION, "Selecciona un contacto", true).setAutoComplete(true);
    return List.of(contacts);
  }

  @Override
  public List<Command.Choice> getAutoCompleteChoices(String optionName, String input) {
    if (!CONTACT_OPTION.equals(optionName)) return List.of();

    return service.getContacts(input).stream()
        .map(contact -> new Command.Choice(choiceName(contact), contact.getId()))
        .toList();
  }

  @Override
  public void execute(SlashCommandInteractionEvent event) {
    int contactId = event.getOption(CONTACT_OPTION).getAsInt();
    ServiceDTO contact = service.getContact(contactId);

    if (event.isFromGuild()) {
      DiscordServerDTO discordServer = super.getServerOwnerInfo(event.getGuild().getIdLong());
      int color = Integer.parseInt(discordServer.getColor().replace("#", ""), 16);
      event.replyEmbeds(embed.buildInfoPanel(color, contact)).setEphemeral(true).queue();
      commandEventService.updateCommandUserCount(
          this.getCommandName(), event.getUser().getName(), event.getGuild().getIdLong());
      return;
    }

    event.replyEmbeds(embed.buildInfoPanel(0x808080, contact)).queue();
  }

  private String choiceName(ServiceDTO contact) {
    String name = contact.getName() == null || contact.getName().isBlank()
        ? "Contact"
        : contact.getName();
    String suffix = " (" + contact.getId() + ")";
    return (name + suffix).length() <= 100
        ? name + suffix
        : name.substring(0, 100 - suffix.length()) + suffix;
  }
}
