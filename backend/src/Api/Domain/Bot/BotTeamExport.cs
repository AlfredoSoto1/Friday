namespace Friday.Backend.Api.Domain;

public sealed class PrepaTeamExportRow
{
  public int TeamId { get; init; }
  public int Position { get; init; }
  public string TeamName { get; init; } = string.Empty;
  public int? ServerUserId { get; init; }
  public string? FirstName { get; init; }
  public string? FirstLastName { get; init; }
  public string? SecondLastName { get; init; }
  public string? Initial { get; init; }
  public string? Email { get; init; }
  public string? Program { get; init; }
}
