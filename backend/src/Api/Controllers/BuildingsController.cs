using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/buildings")]
[Route("api/v1/inelicom/buildings")]
public sealed class BuildingsController : ControllerBase
{
  private readonly IInelicomService _service;

  public BuildingsController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet]
  public async Task<IActionResult> GetBuildings([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure)
    {
      return reqResult.Send();
    }

    var result = await _service.GetBuildings(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetBuilding([FromRoute] int id)
  {
    var result = await _service.GetBuilding(id);
    return result.Send();
  }

  [HttpPost]
  public async Task<IActionResult> CreateBuilding([FromBody] BuildingRequest request)
  {
    var result = await _service.CreateBuilding(request);
    return result.Send();
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> UpdateBuilding([FromRoute] int id, [FromBody] BuildingRequest request)
  {
    var result = await _service.UpdateBuilding(id, request);
    return result.Send();
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteBuilding([FromRoute] int id)
  {
    var result = await _service.DeleteBuilding(id);
    return result.Send();
  }
}
