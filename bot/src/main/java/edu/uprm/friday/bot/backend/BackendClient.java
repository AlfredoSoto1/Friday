package edu.uprm.friday.bot.backend;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import edu.uprm.friday.bot.backend.dto.BotCommandResponse;
import edu.uprm.friday.bot.backend.dto.BotGuildProfile;
import edu.uprm.friday.bot.backend.dto.BotGuildSummary;
import edu.uprm.friday.bot.backend.dto.BotSyncResult;
import edu.uprm.friday.bot.backend.dto.BotSyncRequest;
import edu.uprm.friday.bot.backend.dto.BotVerifyMemberRequest;
import edu.uprm.friday.bot.backend.dto.BotVerifyMemberResult;
import edu.uprm.friday.bot.backend.dto.BotXpRequest;
import edu.uprm.friday.bot.backend.dto.BotXpResult;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.List;
import java.util.Optional;

public final class BackendClient {
  private static final Logger LOGGER = LoggerFactory.getLogger(BackendClient.class);

  private final String baseUrl;
  private final HttpClient httpClient;
  private final ObjectMapper objectMapper;

  public BackendClient(String baseUrl) {
    this.baseUrl = baseUrl;
    this.httpClient = HttpClient.newBuilder()
      .connectTimeout(Duration.ofSeconds(3))
      .build();
    this.objectMapper = new ObjectMapper()
      .setPropertyNamingStrategy(PropertyNamingStrategies.SNAKE_CASE);
  }

  public List<BotGuildSummary> enabledGuilds() {
    return get("/api/v1/bot/servers")
      .map(data -> objectMapper.convertValue(data, new TypeReference<List<BotGuildSummary>>() {}))
      .orElse(List.of());
  }

  public BotGuildProfile guildProfile(long guildId) {
    return get("/api/v1/bot/servers/" + guildId + "/profile")
      .map(data -> objectMapper.convertValue(data, BotGuildProfile.class))
      .orElseGet(() -> BotGuildProfile.defaultFor(guildId));
  }

  public BotCommandResponse commandResponse(long guildId, String commandName) {
    return get("/api/v1/bot/servers/" + guildId + "/commands/" + commandName)
      .map(data -> objectMapper.convertValue(data, BotCommandResponse.class))
      .orElseGet(() -> BotCommandResponse.defaultFor(commandName));
  }

  public BotVerifyMemberResult verifyMember(long guildId, BotVerifyMemberRequest request) {
    return post("/api/v1/bot/servers/" + guildId + "/members/verify", request)
      .map(data -> objectMapper.convertValue(data, BotVerifyMemberResult.class))
      .orElseGet(() -> new BotVerifyMemberResult(false, "Backend verification is unavailable.", List.of()));
  }

  public BotSyncResult syncGuild(BotSyncRequest request) {
    return post("/api/v1/bot/servers/" + request.guildId() + "/sync", request)
      .map(data -> objectMapper.convertValue(data, BotSyncResult.class))
      .orElseGet(() -> BotSyncResult.unavailable(request.guildId()));
  }

  public BotXpResult addXp(long guildId, BotXpRequest request) {
    return post("/api/v1/bot/servers/" + guildId + "/members/xp", request)
      .map(data -> objectMapper.convertValue(data, BotXpResult.class))
      .orElseGet(() -> new BotXpResult(request.discordUserId(), 0, 1, false));
  }

  public boolean isGuildEnabled(long guildId) {
    return guildProfile(guildId).enabled();
  }

  private Optional<JsonNode> get(String path) {
    HttpRequest request = HttpRequest.newBuilder(URI.create(baseUrl + path))
      .timeout(Duration.ofSeconds(5))
      .GET()
      .build();
    return send(request);
  }

  private Optional<JsonNode> post(String path, Object body) {
    try {
      HttpRequest request = HttpRequest.newBuilder(URI.create(baseUrl + path))
        .timeout(Duration.ofSeconds(5))
        .header("Content-Type", "application/json")
        .POST(HttpRequest.BodyPublishers.ofString(objectMapper.writeValueAsString(body)))
        .build();
      return send(request);
    } catch (IOException ex) {
      LOGGER.warn("Failed serializing backend request {}: {}", path, ex.getMessage());
      return Optional.empty();
    }
  }

  private Optional<JsonNode> send(HttpRequest request) {
    try {
      HttpResponse<String> response = httpClient.send(request, HttpResponse.BodyHandlers.ofString());
      if (response.statusCode() < 200 || response.statusCode() >= 300) {
        LOGGER.warn("Backend request failed: {} {}", response.statusCode(), request.uri());
        return Optional.empty();
      }

      JsonNode envelope = objectMapper.readTree(response.body());
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
}
