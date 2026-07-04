package assistant.app.interactions;

import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.events.guild.member.GuildMemberJoinEvent;
import net.dv8tion.jda.api.events.message.MessageReceivedEvent;
import net.dv8tion.jda.api.events.session.ReadyEvent;

import java.util.LinkedHashMap;
import java.util.Map;

public abstract class InteractionDefinition {
  private final Map<String, ButtonInteractionHandler> buttons = new LinkedHashMap<>();
  private final Map<String, ModalInteractionHandler> modals = new LinkedHashMap<>();
  private final Map<String, SelectMenuInteractionHandler> selectMenus = new LinkedHashMap<>();

  public Map<String, ButtonInteractionHandler> buttons() {
    return buttons;
  }

  public Map<String, ModalInteractionHandler> modals() {
    return modals;
  }

  public Map<String, SelectMenuInteractionHandler> selectMenus() {
    return selectMenus;
  }

  public void onReady(ReadyEvent event) {
  }

  public void onGuildReady(Guild guild) {
  }

  public void onMemberJoin(GuildMemberJoinEvent event) {
  }

  public void onMessageReceived(MessageReceivedEvent event) {
  }

  protected void registerButton(String componentId, ButtonInteractionHandler handler) {
    buttons.put(componentId, handler);
  }

  protected void registerModal(String modalId, ModalInteractionHandler handler) {
    modals.put(modalId, handler);
  }

  protected void registerSelectMenu(String componentId, SelectMenuInteractionHandler handler) {
    selectMenus.put(componentId, handler);
  }
}
