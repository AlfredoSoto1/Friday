namespace Friday.Backend.Api.Domain;

public sealed class VerifyMemberRequest
{
  public string DiscordUserId { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string? FunFact { get; init; }
}

public sealed class MemberVerification
{
  public bool Verified { get; init; }
  public string Message { get; init; } = string.Empty;
  public IReadOnlyCollection<string> RoleIds { get; init; } = [];
}

public sealed class XpRequest
{
  public string DiscordUserId { get; init; } = string.Empty;
}

public sealed class MemberXp
{
  public string DiscordUserId { get; init; } = string.Empty;
  public int Xp { get; init; }
  public int Level { get; init; }
  public bool LeveledUp { get; init; }
}
