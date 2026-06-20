using System.Text.Json;

namespace Utils;

public interface IApiResponse
{
  MetaEnvelope Meta { get; set; }
}

public class ApiResponse<T> : IApiResponse
{
  public string Status { get; set; } = "success";
  public MetaEnvelope Meta { get; set; } = new();
  public ErrorInfo? Error { get; set; }
  public T? Data { get; set; }
}

public class MetaEnvelope
{
  public DateTimeOffset? Timestamp { get; set; }
  public string? RequestId { get; set; }
  public long? ProcessingTimeMs { get; set; }
  public int? Limit { get; set; }
  public int? PageIndex { get; set; }
  public long? Total { get; set; }
  public long? Remaining { get; set; }
  public JsonElement? Custom { get; set; }
}

public record ChannelCount(string Channel, long Count);

public class ErrorInfo
{
  public int Code { get; set; }
  public string Message { get; set; } = string.Empty;
  public object? Details { get; set; }
}

public readonly record struct Paged<T>(IEnumerable<T> Items, long Total, int Remaining = 0)
{
  public static Paged<T> Empty => new(Enumerable.Empty<T>(), 0);
}

public readonly record struct PagedExt<T, M>(IEnumerable<T> Items, long Total, M Meta)
{
  public static PagedExt<T, M> Empty => new(Enumerable.Empty<T>(), 0, default!);
}
