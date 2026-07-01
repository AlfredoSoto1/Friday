namespace Friday.Backend.Api.Domain;

public sealed class RosterStudentRequest
{
  public string Email { get; init; } = string.Empty;
  public string Fullname { get; init; } = string.Empty;
  public string Username { get; init; } = string.Empty;
  public string TeamName { get; init; } = string.Empty;
}

public sealed class SaveGuildRosterRequest
{
  public IReadOnlyCollection<string> TeamNames { get; init; } = [];
  public IReadOnlyCollection<RosterStudentRequest> Students { get; init; } = [];
}

public sealed class RosterUserReference
{
  public int UserId { get; init; }
  public string Email { get; init; } = string.Empty;
}

public sealed class RosterMemberReference
{
  public int ServerUserId { get; init; }
  public int UserId { get; init; }
}

public sealed class RosterTeamReference
{
  public int TeamId { get; init; }
  public string Name { get; init; } = string.Empty;
}

public sealed class SaveGuildRosterResult
{
  public int StudentCount { get; init; }
  public int TeamCount { get; init; }
}
