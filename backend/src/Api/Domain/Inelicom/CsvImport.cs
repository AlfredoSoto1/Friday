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

public sealed class ImportCounter
{
  public int Inserted { get; private set; }
  public int Updated { get; private set; }
  public int Skipped { get; private set; }
  public List<string> Errors { get; } = new();

  public void AddInserted() => Inserted++;
  public void AddUpdated() => Updated++;
  public void AddSkipped(string message)
  {
    Skipped++;
    Errors.Add(message);
  }
}
