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
package assistant.app.discord;

import assistant.app.discord.core.ListenerAdapterManager;
import assistant.app.interactions.CommandI;
import assistant.app.interactions.InteractionModel;
import assistant.commands.contacts.ContactCmd;
import assistant.commands.files.CurriculumCmd;
import assistant.commands.files.FreshmanGuideCmd;
import assistant.commands.games.CoinFlipCmd;
import assistant.commands.games.DiceCmd;
import assistant.commands.games.EightBallCmd;
import assistant.commands.games.GamesCmd;
import assistant.commands.games.LeaderboardCmd;
import assistant.commands.games.PingCmd;
import assistant.commands.games.RockPaperScissorsCmd;
import assistant.commands.information.CalendarCmd;
import assistant.commands.information.EOInfoCmd;
import assistant.commands.information.FAQCmd;
import assistant.commands.information.FacultyCmd;
import assistant.commands.information.FindBuildingCmd;
import assistant.commands.information.HelpCmd;
import assistant.commands.information.OrgsCmd;
import assistant.commands.information.ProjectsCmd;
import assistant.commands.information.RulesCmd;
import assistant.commands.information.UprmMapCmd;
import assistant.commands.links.LinksCmd;
import assistant.commands.links.MadeWebCmd;
import assistant.commands.moderation.AssistantCmd;
import assistant.commands.moderation.VerificationCmd;
import assistant.interactions.games.ProfanityMessenger;
import assistant.interactions.moderation.WelcomeMessenger;
import java.util.List;

/** */
public final class ECEAssistant extends BotApplication {
  private final List<InteractionModel> interactions;

  public ECEAssistant(String botToken) {
    super(botToken);
    interactions = List.of(
        new FacultyCmd(),
        global(new ProjectsCmd()),
        global(new OrgsCmd()),
        new EOInfoCmd(),
        global(new FindBuildingCmd()),
        new FAQCmd(),
        global(new HelpCmd()),
        global(new UprmMapCmd()),
        global(new RulesCmd()),
        global(new CalendarCmd()),
        new MadeWebCmd(),
        global(new LinksCmd()),
        new ContactCmd(),
        new CurriculumCmd(),
        global(new FreshmanGuideCmd()),
        new AssistantCmd(this),
        new VerificationCmd(),
        new WelcomeMessenger(),
        new GamesCmd(),
        new CoinFlipCmd(),
        new PingCmd(),
        new DiceCmd(),
        new RockPaperScissorsCmd(),
        new EightBallCmd(),
        new LeaderboardCmd(),
        new ProfanityMessenger());
  }

  @Override
  protected void prepareListeners(ListenerAdapterManager listener) {
    listener.registerInteractions(interactions);
  }

  @Override
  protected void dispose() {
    // The listener owns interaction lifecycle cleanup.
  }

  private <T extends InteractionModel & CommandI> T global(T command) {
    command.setGlobal(true);
    return command;
  }
}
