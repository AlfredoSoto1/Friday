namespace Friday.Backend.Api.Domain;

public sealed class Faculty
{
  public int FacultyId { get; init; }
  public string Name { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public sealed class FacultyRequest
{
  public string Name { get; init; } = string.Empty;
}
