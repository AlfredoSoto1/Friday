package assistant.commands.games;

import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.embeds.games.GameEmbed;
import java.util.List;
import net.dv8tion.jda.api.entities.Guild;
import net.dv8tion.jda.api.interactions.commands.build.OptionData;

abstract class GameCommand extends InteractionModel implements CommandI {
  protected final GameEmbed embed = new GameEmbed();

  @Override
  public final boolean isGlobal() {
    return true;
  }

  @Override
  public final void setGlobal(boolean isGlobal) {
    // Game commands are available globally, including bot direct messages.
  }

  @Override
  public List<OptionData> getOptions(Guild guild) {
    return List.of();
  }
}
