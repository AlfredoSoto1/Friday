namespace Friday.Backend.Api.Domain;

public sealed class Project
{
  public int ProjectId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public sealed class ProjectRequest
{
  public string Name { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
}
