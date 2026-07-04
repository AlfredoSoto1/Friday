package assistant.backend.service;

import assistant.backend.BackendClient;
import assistant.backend.dto.UserRankDTO;
import assistant.backend.dto.BotXpRequest;
import com.fasterxml.jackson.databind.JsonNode;
import java.util.*;
import java.util.stream.StreamSupport;

public final class GameService {
  public void updateCommandUserCount(String command,String user,long server) { }
  public Optional<UserRankDTO> giveXP(String user,int quantity,long server){ var r=BackendClient.addXp(server,new BotXpRequest(user)); UserRankDTO d=new UserRankDTO(); d.setUsername(user); d.setUserXP(r.xp()); d.setLevel(r.level()); d.setHasLevelup(r.leveledUp()); return Optional.of(d); }
  public List<UserRankDTO> getLeaderboard(long server){ List<UserRankDTO> result=new ArrayList<>(); BackendClient.getData("/api/v1/bot/servers/"+server+"/members").ifPresent(n->{ int[] rank={1}; StreamSupport.stream(n.spliterator(),false).sorted(Comparator.comparingInt(x->-x.path("xp").asInt())).forEach(x->{ UserRankDTO d=map(x); d.setRank(rank[0]++); result.add(d); }); }); return result; }
  public Optional<UserRankDTO> getUserLeaderboardPosition(String user,long server){ return getLeaderboard(server).stream().filter(x->x.getUsername().equalsIgnoreCase(user)).findFirst(); }
  private UserRankDTO map(JsonNode n){ UserRankDTO d=new UserRankDTO(); d.setUsername(n.path("username").asText()); d.setUserXP(n.path("xp").asInt()); d.setLevel(n.path("level").asInt()); return d; }
}
