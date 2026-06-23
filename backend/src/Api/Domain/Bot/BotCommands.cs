namespace Friday.Backend.Api.Domain;

public sealed class BotCommandResponse
{
  public string CommandName { get; init; } = string.Empty;
  public string Title { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public string? ImageUrl { get; init; }
  public string? Url { get; init; }
  public string? Color { get; init; }
  public bool Ephemeral { get; init; } = true;
  public IReadOnlyCollection<BotButtonDefinition> Buttons { get; init; } = [];
}

public sealed class BotButtonDefinition
{
  public string Id { get; init; } = string.Empty;
  public string Label { get; init; } = string.Empty;
  public string Style { get; init; } = "primary";
  public string? Url { get; init; }
}
