namespace Friday.Backend.Api.Domain;

public sealed class Faculty
{
  public int FacultyId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? Extension { get; init; }
  public string? Web { get; init; }
  public string? Phone { get; init; }
  public string? Facebook { get; init; }
  public string? Email { get; init; }
  public string? Office { get; init; }
  public string? JobEntitlement { get; init; }
  public string? Description { get; init; }
  public string? Abbreviation { get; init; }
  public string? Instagram { get; init; }
  public DateTime CreatedAt { get; init; }
}

public sealed class FacultyRequest
{
  public string Name { get; init; } = string.Empty;
  public string? Extension { get; init; }
  public string? Web { get; init; }
  public string? Phone { get; init; }
  public string? Facebook { get; init; }
  public string? Email { get; init; }
  public string? Office { get; init; }
  public string? JobEntitlement { get; init; }
  public string? Description { get; init; }
  public string? Abbreviation { get; init; }
  public string? Instagram { get; init; }
}
