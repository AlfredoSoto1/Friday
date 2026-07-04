package assistant.backend;

import assistant.backend.dto.BotCommandResponse;
import assistant.backend.dto.BotGuildProfile;
import assistant.backend.dto.BotGuildSummary;
import assistant.backend.dto.BotSyncRequest;
import assistant.backend.dto.BotSyncResult;
import assistant.backend.dto.BotVerifyMemberRequest;
import assistant.backend.dto.BotVerifyMemberResult;
import assistant.backend.dto.BotXpRequest;
import assistant.backend.dto.BotXpResult;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.List;
import java.util.Optional;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public final class BackendClient {
  private static final Logger LOGGER = LoggerFactory.getLogger(BackendClient.class);
  private static final HttpClient HTTP_CLIENT =
      HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(3)).build();
  private static final ObjectMapper OBJECT_MAPPER =
      new ObjectMapper()
          .setPropertyNamingStrategy(PropertyNamingStrategies.SNAKE_CASE)
          .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
  private static String baseUrl;

  private BackendClient() {}

  public static void configure(String backendUrl) {
    if (backendUrl == null || backendUrl.isBlank()) {
      throw new IllegalArgumentException("Backend URL must not be blank.");
    }
    baseUrl = backendUrl;
  }

  public static List<BotGuildSummary> enabledGuilds() {
    return get("/api/v1/bot/servers")
        .map(
            data -> OBJECT_MAPPER.convertValue(data, new TypeReference<List<BotGuildSummary>>() {}))
        .orElse(List.of());
  }

  public static BotGuildProfile guildProfile(long guildId) {
    return get("/api/v1/bot/servers/" + guildId + "/profile")
        .map(data -> OBJECT_MAPPER.convertValue(data, BotGuildProfile.class))
        .orElseGet(() -> BotGuildProfile.defaultFor(guildId));
  }

  public static BotCommandResponse commandResponse(long guildId, String commandName) {
    return get("/api/v1/bot/servers/" + guildId + "/commands/" + commandName)
        .map(data -> OBJECT_MAPPER.convertValue(data, BotCommandResponse.class))
        .orElseGet(() -> BotCommandResponse.defaultFor(commandName));
  }

  public static BotVerifyMemberResult verifyMember(long guildId, BotVerifyMemberRequest request) {
    return post("/api/v1/bot/servers/" + guildId + "/members/verify", request)
        .map(data -> OBJECT_MAPPER.convertValue(data, BotVerifyMemberResult.class))
        .orElseGet(
            () ->
                new BotVerifyMemberResult(
                    false, "Backend verification is unavailable.", List.of()));
  }

  public static BotSyncResult syncGuild(BotSyncRequest request) {
    return post("/api/v1/bot/servers/" + request.guildId() + "/sync", request)
        .map(data -> OBJECT_MAPPER.convertValue(data, BotSyncResult.class))
        .orElseGet(() -> BotSyncResult.unavailable(request.guildId()));
  }

  public static BotXpResult addXp(long guildId, BotXpRequest request) {
    return post("/api/v1/bot/servers/" + guildId + "/members/xp", request)
        .map(data -> OBJECT_MAPPER.convertValue(data, BotXpResult.class))
        .orElseGet(() -> new BotXpResult(request.discordUserId(), 0, 1, false));
  }

  public static boolean isGuildEnabled(long guildId) {
    return guildProfile(guildId).enabled();
  }

  public static Optional<JsonNode> getData(String path) {
    return get(path);
  }

  public static Optional<JsonNode> postData(String path, Object body) {
    return post(path, body);
  }

  public static ObjectMapper mapper() {
    return OBJECT_MAPPER;
  }

  private static Optional<JsonNode> get(String path) {
    HttpRequest request =
        HttpRequest.newBuilder(uri(path)).timeout(Duration.ofSeconds(5)).GET().build();
    return send(request);
  }

  private static Optional<JsonNode> post(String path, Object body) {
    try {
      HttpRequest request =
          HttpRequest.newBuilder(uri(path))
              .timeout(Duration.ofSeconds(5))
              .header("Content-Type", "application/json")
              .POST(HttpRequest.BodyPublishers.ofString(OBJECT_MAPPER.writeValueAsString(body)))
              .build();
      return send(request);
    } catch (IOException ex) {
      LOGGER.warn("Failed serializing backend request {}: {}", path, ex.getMessage());
      return Optional.empty();
    }
  }

  private static Optional<JsonNode> send(HttpRequest request) {
    try {
      HttpResponse<String> response =
          HTTP_CLIENT.send(request, HttpResponse.BodyHandlers.ofString());
      if (response.statusCode() < 200 || response.statusCode() >= 300) {
        LOGGER.warn("Backend request failed: {} {}", response.statusCode(), request.uri());
        return Optional.empty();
      }

      JsonNode envelope = OBJECT_MAPPER.readTree(response.body());
      JsonNode data = envelope.get("data");
      return data == null || data.isNull() ? Optional.empty() : Optional.of(data);
    } catch (IOException | InterruptedException ex) {
      if (ex instanceof InterruptedException) {
        Thread.currentThread().interrupt();
      }
      LOGGER.warn("Backend request unavailable: {} ({})", request.uri(), ex.getMessage());
      return Optional.empty();
    }
  }

  private static URI uri(String path) {
    if (baseUrl == null) {
      throw new IllegalStateException("BackendClient must be configured before use.");
    }
    return URI.create(baseUrl + path);
  }
}
