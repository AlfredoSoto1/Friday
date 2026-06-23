using System.Diagnostics;

namespace Utils;

public static class RequestTiming
{
  private static readonly AsyncLocal<long?> StartedAt = new();

  public static void Start()
  {
    StartedAt.Value = Stopwatch.GetTimestamp();
  }

  public static void Clear()
  {
    StartedAt.Value = null;
  }

  public static long? ElapsedMilliseconds
  {
    get
    {
      if (!StartedAt.Value.HasValue)
      {
        return null;
      }

      return (long)Stopwatch.GetElapsedTime(StartedAt.Value.Value).TotalMilliseconds;
    }
  }
}
