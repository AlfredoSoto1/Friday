using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Route("api/v1/dashboard")]
public sealed class DashboardController : ControllerBase
{
  private readonly IDashboardService _service;

  public DashboardController(IDashboardService service)
  {
    _service = service;
  }

  [HttpGet]
  public async Task<IActionResult> GetDashboardContent([FromQuery] DashboardQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure)
    {
      return reqResult.Send();
    }

    var result = await _service.GetDashboardContent(reqResult.Value);
    return result.Send();
  }

  [HttpPost("servers")]
  public async Task<IActionResult> CreateDiscordServer([FromBody] CreateDiscordServerRequest request)
  {
    var result = await _service.CreateDiscordServer(request);
    return result.Send();
  }

  [HttpPatch("servers/{serverId:int}/enabled")]
  public async Task<IActionResult> SetDiscordServerEnabled(
    [FromRoute] int serverId,
    [FromBody] SetDiscordServerEnabledRequest request)
  {
    var result = await _service.SetDiscordServerEnabled(serverId, request.Enabled);
    return result.Send();
  }

  [HttpDelete("servers/{serverId:int}")]
  public async Task<IActionResult> DeleteDiscordServer([FromRoute] int serverId)
  {
    var result = await _service.DeleteDiscordServer(serverId);
    return result.Send();
  }

  [HttpGet("/api/status")]
  [HttpGet("/api/v1/status")]
  public async Task<IActionResult> GetStatus()
  {
    var result = await _service.GetStatus();
    return result.Send();
  }

  [HttpGet("/api/catalog/summary")]
  [HttpGet("/api/v1/catalog/summary")]
  public async Task<IActionResult> GetCatalogSummary()
  {
    var result = await _service.GetCatalogSummary();
    return result.Send();
  }

}
