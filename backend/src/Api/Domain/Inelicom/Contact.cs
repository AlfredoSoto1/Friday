namespace Friday.Backend.Api.Domain;

public sealed class Contact
{
  public int ContactId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string Phone { get; init; } = string.Empty;
  public string Website { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public string Services { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}

public sealed class ContactRequest
{
  public string Name { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string Phone { get; init; } = string.Empty;
  public string Website { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public string Services { get; init; } = string.Empty;
}
