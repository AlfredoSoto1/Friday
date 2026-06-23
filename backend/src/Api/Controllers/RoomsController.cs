using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/rooms")]
[Route("api/v1/inelicom/rooms")]
public sealed class RoomsController : ControllerBase
{
  private readonly IInelicomService _service;

  public RoomsController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet]
  public async Task<IActionResult> GetRooms([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure)
    {
      return reqResult.Send();
    }

    var result = await _service.GetRooms(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetRoom([FromRoute] int id)
  {
    var result = await _service.GetRoom(id);
    return result.Send();
  }

  [HttpPost]
  public async Task<IActionResult> CreateRoom([FromBody] RoomRequest request)
  {
    var result = await _service.CreateRoom(request);
    return result.Send();
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> UpdateRoom([FromRoute] int id, [FromBody] RoomRequest request)
  {
    var result = await _service.UpdateRoom(id, request);
    return result.Send();
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteRoom([FromRoute] int id)
  {
    var result = await _service.DeleteRoom(id);
    return result.Send();
  }
}
