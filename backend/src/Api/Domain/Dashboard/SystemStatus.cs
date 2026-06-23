namespace Friday.Backend.Api.Domain;

public sealed class BackendStatus
{
  public string Status { get; init; } = "ok";
  public string Database { get; init; } = "unknown";
  public DateTimeOffset CheckedAt { get; init; }
}

public sealed class CatalogSummary
{
  public IReadOnlyCollection<SchemaSummary> Schemas { get; init; } = [];
}

public sealed class SchemaSummary
{
  public string Name { get; init; } = string.Empty;
  public IReadOnlyCollection<TableSummary> Tables { get; init; } = [];
}

public sealed class TableSummary
{
  public string Schema { get; init; } = string.Empty;
  public string Table { get; init; } = string.Empty;
  public long Rows { get; init; }
}
