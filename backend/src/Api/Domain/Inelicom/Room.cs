namespace Friday.Backend.Api.Domain;

public sealed class Room
{
  public int RoomId { get; init; }
  public string Code { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public int BuildingId { get; init; }
  public int DepartmentId { get; init; }
  public DateTime CreatedAt { get; init; }
}

public sealed class RoomRequest
{
  public string Code { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public int BuildingId { get; init; }
  public int DepartmentId { get; init; }
}
