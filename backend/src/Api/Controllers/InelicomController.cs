using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom")]
[Route("api/v1/inelicom")]
public sealed class InelicomController : ControllerBase
{
  private readonly IInelicomService _service;

  public InelicomController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet("buildings")]
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

  [HttpGet("buildings/{id:int}")]
  public async Task<IActionResult> GetBuilding([FromRoute] int id)
  {
    var result = await _service.GetBuilding(id);
    return result.Send();
  }

  [HttpGet("faculties")]
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

  [HttpGet("faculties/{id:int}")]
  public async Task<IActionResult> GetFaculty([FromRoute] int id)
  {
    var result = await _service.GetFaculty(id);
    return result.Send();
  }

  [HttpGet("departments")]
  public async Task<IActionResult> GetDepartments([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure)
    {
      return reqResult.Send();
    }

    var result = await _service.GetDepartments(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("departments/{id:int}")]
  public async Task<IActionResult> GetDepartment([FromRoute] int id)
  {
    var result = await _service.GetDepartment(id);
    return result.Send();
  }

  [HttpGet("contacts")]
  public async Task<IActionResult> GetContacts([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure)
    {
      return reqResult.Send();
    }

    var result = await _service.GetContacts(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("contacts/{id:int}")]
  public async Task<IActionResult> GetContact([FromRoute] int id)
  {
    var result = await _service.GetContact(id);
    return result.Send();
  }

  [HttpGet("organizations")]
  public async Task<IActionResult> GetOrganizations([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure)
    {
      return reqResult.Send();
    }

    var result = await _service.GetOrganizations(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("organizations/{id:int}")]
  public async Task<IActionResult> GetOrganization([FromRoute] int id)
  {
    var result = await _service.GetOrganization(id);
    return result.Send();
  }

  [HttpGet("projects")]
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

  [HttpGet("projects/{id:int}")]
  public async Task<IActionResult> GetProject([FromRoute] int id)
  {
    var result = await _service.GetProject(id);
    return result.Send();
  }
}
