using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/organizations")]
[Route("api/v1/inelicom/organizations")]
public sealed class OrganizationsController : ControllerBase
{
  private readonly IInelicomService _service;

  public OrganizationsController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet]
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

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetOrganization([FromRoute] int id)
  {
    var result = await _service.GetOrganization(id);
    return result.Send();
  }

  [HttpPost]
  public async Task<IActionResult> CreateOrganization([FromBody] OrganizationRequest request)
  {
    var result = await _service.CreateOrganization(request);
    return result.Send();
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> UpdateOrganization([FromRoute] int id, [FromBody] OrganizationRequest request)
  {
    var result = await _service.UpdateOrganization(id, request);
    return result.Send();
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteOrganization([FromRoute] int id)
  {
    var result = await _service.DeleteOrganization(id);
    return result.Send();
  }
}
