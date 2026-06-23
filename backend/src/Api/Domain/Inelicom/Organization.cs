namespace Friday.Backend.Api.Domain;

public sealed class Organization
{
  public int OrganizationId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public sealed class OrganizationRequest
{
  public string Name { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
}
