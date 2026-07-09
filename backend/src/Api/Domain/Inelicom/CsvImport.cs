namespace Friday.Backend.Api.Domain;

public sealed class CsvImportResult
{
  public string Kind { get; init; } = string.Empty;
  public string FileName { get; init; } = string.Empty;
  public int Inserted { get; init; }
  public int Updated { get; init; }
  public int Skipped { get; init; }
  public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

public sealed class CsvImportStats
{
  public int Inserted { get; init; }
  public int Updated { get; init; }
  public int Skipped { get; init; }
  public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
