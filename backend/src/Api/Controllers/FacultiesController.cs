using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/faculties")]
[Route("api/v1/inelicom/faculties")]
public sealed class FacultiesController : ControllerBase
{
  private readonly IInelicomService _service;

  public FacultiesController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet]
  public async Task<IActionResult> GetFaculties([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure)
    {
      return reqResult.Send();
    }

    var result = await _service.GetFaculties(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetFaculty([FromRoute] int id)
  {
    var result = await _service.GetFaculty(id);
    return result.Send();
  }

  [HttpPost]
  public async Task<IActionResult> CreateFaculty([FromBody] FacultyRequest request)
  {
    var result = await _service.CreateFaculty(request);
    return result.Send();
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> UpdateFaculty([FromRoute] int id, [FromBody] FacultyRequest request)
  {
    var result = await _service.UpdateFaculty(id, request);
    return result.Send();
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteFaculty([FromRoute] int id)
  {
    var result = await _service.DeleteFaculty(id);
    return result.Send();
  }
}
