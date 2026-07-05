namespace Friday.Backend.Api.Domain;

public sealed class Curriculum
{
  public int CurriculumId { get; init; }
  public string Program { get; init; } = string.Empty;
  public string FileName { get; init; } = string.Empty;
  public string ContentType { get; init; } = string.Empty;
  public long FileSize { get; init; }
  public DateTime UploadedAt { get; init; }
}

public sealed record CurriculumFile(string FileName, string ContentType, byte[] Data);
