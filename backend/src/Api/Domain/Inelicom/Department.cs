namespace Friday.Backend.Api.Domain;

public sealed class Department
{
  public int DepartmentId { get; init; }
  public string Name { get; init; } = string.Empty;
  public int FacultyId { get; init; }
  public int BuildingId { get; init; }
  public DateTime CreatedAt { get; init; }
}

public sealed class DepartmentRequest
{
  public string Name { get; init; } = string.Empty;
  public int FacultyId { get; init; }
  public int BuildingId { get; init; }
}
