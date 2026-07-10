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
  public string DepartmentProfile { get; init; } = string.Empty;
  public bool Enabled { get; init; }
  public DateTime CreatedAt { get; init; }
}

public sealed class CreateDiscordServerRequest
{
  public string Name { get; init; } = string.Empty;
  public string ServerCode { get; init; } = string.Empty;
  public string DepartmentProfile { get; init; } = string.Empty;
}

public sealed class SetDiscordServerEnabledRequest
{
  public bool Enabled { get; init; }
}
