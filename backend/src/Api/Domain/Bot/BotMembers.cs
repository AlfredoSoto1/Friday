namespace Friday.Backend.Api.Domain;

public sealed class BotVerifyMemberRequest
{
  public string DiscordUserId { get; init; } = string.Empty;
  public string DiscordUsername { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string? FunFact { get; init; }
}

public sealed class BotVerifyMemberResult
{
  public bool Verified { get; init; }
  public string Message { get; init; } = string.Empty;
  public IReadOnlyCollection<string> RoleIds { get; init; } = [];
}

public sealed class BotXpRequest
{
  public string DiscordUserId { get; init; } = string.Empty;
  public string DiscordUsername { get; init; } = string.Empty;
  public int Amount { get; init; } = 1;
}

public sealed class BotXpResult
{
  public string DiscordUserId { get; init; } = string.Empty;
  public int Xp { get; init; }
  public int Level { get; init; }
  public bool LeveledUp { get; init; }
}
