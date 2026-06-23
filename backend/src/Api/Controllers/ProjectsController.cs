using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/projects")]
[Route("api/v1/inelicom/projects")]
public sealed class ProjectsController : ControllerBase
{
  private readonly IInelicomService _service;

  public ProjectsController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet]
  public async Task<IActionResult> GetProjects([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure)
    {
      return reqResult.Send();
    }

    var result = await _service.GetProjects(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetProject([FromRoute] int id)
  {
    var result = await _service.GetProject(id);
    return result.Send();
  }

  [HttpPost]
  public async Task<IActionResult> CreateProject([FromBody] ProjectRequest request)
  {
    var result = await _service.CreateProject(request);
    return result.Send();
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> UpdateProject([FromRoute] int id, [FromBody] ProjectRequest request)
  {
    var result = await _service.UpdateProject(id, request);
    return result.Send();
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteProject([FromRoute] int id)
  {
    var result = await _service.DeleteProject(id);
    return result.Send();
  }
}
