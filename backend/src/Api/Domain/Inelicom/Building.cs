namespace Friday.Backend.Api.Domain;

public sealed class Building
{
  public int BuildingId { get; init; }
  public string? Code { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Gpin { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public sealed class BuildingRequest
{
  public string? Code { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Gpin { get; init; } = string.Empty;
}
