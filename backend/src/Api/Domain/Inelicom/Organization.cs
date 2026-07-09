namespace Friday.Backend.Api.Domain;

public sealed class Organization
{
  public int OrganizationId { get; init; }
  public string? Email { get; init; }
  public string? Facebook { get; init; }
  public string? Instagram { get; init; }
  public string? TwitterX { get; init; }
  public string? Web { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public sealed class OrganizationRequest
{
  public string? Email { get; init; }
  public string? Facebook { get; init; }
  public string? Instagram { get; init; }
  public string? TwitterX { get; init; }
  public string? Web { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
}
