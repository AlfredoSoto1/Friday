namespace Friday.Backend.Api.Domain;

public sealed class RosterStudentRequest
{
  public string Email { get; init; } = string.Empty;
  public string FirstName { get; init; } = string.Empty;
  public string FirstLastName { get; init; } = string.Empty;
  public string SecondLastName { get; init; } = string.Empty;
  public string Initial { get; init; } = string.Empty;
}

public sealed class RosterTeamRequest
{
  public int? TeamId { get; init; }
  public string Name { get; init; } = string.Empty;
  public IReadOnlyCollection<int> RoleIds { get; init; } = [];
  public bool AppendMembers { get; init; } = true;
  public IReadOnlyCollection<RosterStudentRequest> Students { get; init; } = [];
}

public sealed class SaveGuildRosterRequest
{
  public IReadOnlyCollection<RosterTeamRequest> Teams { get; init; } = [];
}

public sealed class GuildTeam
{
  public int TeamId { get; init; }
  public int Position { get; init; }
  public string Name { get; init; } = string.Empty;
  public int MemberCount { get; init; }
}

public sealed class SaveGuildRosterResult
{
  public int StudentCount { get; init; }
  public int TeamCount { get; init; }
}
