using Utils;

namespace Friday.Backend.Api.Domain;

public sealed class InelicomQuery : BaseUrlQuery
{
  public string? Search { get; init; }
}
