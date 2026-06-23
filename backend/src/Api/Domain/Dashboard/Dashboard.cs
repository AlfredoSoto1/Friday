using Utils;

namespace Friday.Backend.Api.Domain;

public sealed class DashboardQuery : BaseUrlQuery
{
  public string? Search { get; init; }
}

public sealed class DashboardContent
{
  public IReadOnlyCollection<DiscordServer> Servers { get; init; } = [];
}

public sealed class DiscordServer
{
  public int ServerId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string ServerCode { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
}
