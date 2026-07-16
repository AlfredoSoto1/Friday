using System.Text.Json.Serialization;

namespace Friday.Backend.Api.Domain;

public sealed class RosterStudentRequest
{
  public string Email { get; init; } = string.Empty;
  public string FirstName { get; init; } = string.Empty;
  public string FirstLastName { get; init; } = string.Empty;
  public string SecondLastName { get; init; } = string.Empty;
  public string Initial { get; init; } = string.Empty;
  public string Program { get; init; } = string.Empty;
}

public sealed class RosterTeamRequest
{
  public int? TeamId { get; init; }
  public string Name { get; init; } = string.Empty;
  public int? RoleId { get; init; }
  public IReadOnlyCollection<int> RoleIds { get; init; } = [];
  public bool AppendMembers { get; init; }
  public IReadOnlyCollection<RosterStudentRequest> Students { get; init; } = [];

  [JsonIgnore]
  public IReadOnlyCollection<int> SelectedRoleIds =>
    RoleIds.Count > 0 ? RoleIds : RoleId is int roleId ? [roleId] : [];
}

public sealed class SaveGuildRosterRequest
{
  public IReadOnlyCollection<RosterTeamRequest> Teams { get; init; } = [];
}

public sealed class RosterStudentAssignment
{
  public string Email { get; init; } = string.Empty;
  public string FirstName { get; init; } = string.Empty;
  public string FirstLastName { get; init; } = string.Empty;
  public string SecondLastName { get; init; } = string.Empty;
  public string Initial { get; init; } = string.Empty;
  public string Program { get; init; } = string.Empty;
  public string TeamName { get; init; } = string.Empty;
}

public sealed class RosterUserReference
{
  public int UserId { get; init; }
  public string Email { get; init; } = string.Empty;
}

public sealed class RosterMembersContextResult
{
  public IReadOnlyCollection<RosterUserReference> Users { get; init; } = [];
  public IReadOnlyCollection<RosterMemberReference> Members { get; init; } = [];
}

public sealed class RosterMemberReference
{
  public int ServerUserId { get; init; }
  public int UserId { get; init; }
}

public sealed class RosterTeamReference
{
  public int TeamId { get; init; }
  public IReadOnlyCollection<int> RoleIds { get; init; } = [];
  public IReadOnlyCollection<int> PreviousRoleIds { get; init; } = [];
  public string Name { get; init; } = string.Empty;
  public bool AppendMembers { get; init; }
}

public sealed class GuildTeam
{
  public int TeamId { get; init; }
  public int Position { get; init; }
  public string Name { get; init; } = string.Empty;
  public int? RoleId { get; init; }
  public int[] RoleIds { get; init; } = [];
  public string? RoleName { get; init; }
  public int MemberCount { get; init; }
}

public sealed class SaveGuildRosterResult
{
  public int StudentCount { get; init; }
  public int TeamCount { get; init; }
}
