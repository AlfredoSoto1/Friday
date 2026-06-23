using Utils;

namespace Friday.Backend.Api.Domain;

public sealed class InelicomQuery : BaseUrlQuery
{
  public string? Search { get; init; }
}

public sealed class Contact
{
  public int ContactId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string Phone { get; init; } = string.Empty;
  public string Website { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public sealed class ContactRequest
{
  public string Name { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string Phone { get; init; } = string.Empty;
  public string Website { get; init; } = string.Empty;
}

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

public sealed class Building
{
  public int BuildingId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Gpin { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public sealed class BuildingRequest
{
  public string Name { get; init; } = string.Empty;
  public string Gpin { get; init; } = string.Empty;
}

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
