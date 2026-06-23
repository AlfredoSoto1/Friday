using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/departments")]
[Route("api/v1/inelicom/departments")]
public sealed class DepartmentsController : ControllerBase
{
  private readonly IInelicomService _service;

  public DepartmentsController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet]
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

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetDepartment([FromRoute] int id)
  {
    var result = await _service.GetDepartment(id);
    return result.Send();
  }

  [HttpPost]
  public async Task<IActionResult> CreateDepartment([FromBody] DepartmentRequest request)
  {
    var result = await _service.CreateDepartment(request);
    return result.Send();
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> UpdateDepartment([FromRoute] int id, [FromBody] DepartmentRequest request)
  {
    var result = await _service.UpdateDepartment(id, request);
    return result.Send();
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteDepartment([FromRoute] int id)
  {
    var result = await _service.DeleteDepartment(id);
    return result.Send();
  }
}
