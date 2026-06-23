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

  [HttpGet("contacts")]
  public async Task<IActionResult> GetContacts([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure) return reqResult.Send();
    var result = await _service.GetContacts(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("contacts/{id:int}")]
  public async Task<IActionResult> GetContact([FromRoute] int id) => (await _service.GetContact(id)).Send();

  [HttpPost("contacts")]
  public async Task<IActionResult> CreateContact([FromBody] ContactRequest request) => (await _service.CreateContact(request)).Send();

  [HttpPut("contacts/{id:int}")]
  public async Task<IActionResult> UpdateContact([FromRoute] int id, [FromBody] ContactRequest request) => (await _service.UpdateContact(id, request)).Send();

  [HttpDelete("contacts/{id:int}")]
  public async Task<IActionResult> DeleteContact([FromRoute] int id) => (await _service.DeleteContact(id)).Send();

  [HttpGet("faculties")]
  public async Task<IActionResult> GetFaculties([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure) return reqResult.Send();
    var result = await _service.GetFaculties(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("faculties/{id:int}")]
  public async Task<IActionResult> GetFaculty([FromRoute] int id) => (await _service.GetFaculty(id)).Send();

  [HttpPost("faculties")]
  public async Task<IActionResult> CreateFaculty([FromBody] FacultyRequest request) => (await _service.CreateFaculty(request)).Send();

  [HttpPut("faculties/{id:int}")]
  public async Task<IActionResult> UpdateFaculty([FromRoute] int id, [FromBody] FacultyRequest request) => (await _service.UpdateFaculty(id, request)).Send();

  [HttpDelete("faculties/{id:int}")]
  public async Task<IActionResult> DeleteFaculty([FromRoute] int id) => (await _service.DeleteFaculty(id)).Send();

  [HttpGet("buildings")]
  public async Task<IActionResult> GetBuildings([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure) return reqResult.Send();
    var result = await _service.GetBuildings(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("buildings/{id:int}")]
  public async Task<IActionResult> GetBuilding([FromRoute] int id) => (await _service.GetBuilding(id)).Send();

  [HttpPost("buildings")]
  public async Task<IActionResult> CreateBuilding([FromBody] BuildingRequest request) => (await _service.CreateBuilding(request)).Send();

  [HttpPut("buildings/{id:int}")]
  public async Task<IActionResult> UpdateBuilding([FromRoute] int id, [FromBody] BuildingRequest request) => (await _service.UpdateBuilding(id, request)).Send();

  [HttpDelete("buildings/{id:int}")]
  public async Task<IActionResult> DeleteBuilding([FromRoute] int id) => (await _service.DeleteBuilding(id)).Send();

  [HttpGet("departments")]
  public async Task<IActionResult> GetDepartments([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure) return reqResult.Send();
    var result = await _service.GetDepartments(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("departments/{id:int}")]
  public async Task<IActionResult> GetDepartment([FromRoute] int id) => (await _service.GetDepartment(id)).Send();

  [HttpPost("departments")]
  public async Task<IActionResult> CreateDepartment([FromBody] DepartmentRequest request) => (await _service.CreateDepartment(request)).Send();

  [HttpPut("departments/{id:int}")]
  public async Task<IActionResult> UpdateDepartment([FromRoute] int id, [FromBody] DepartmentRequest request) => (await _service.UpdateDepartment(id, request)).Send();

  [HttpDelete("departments/{id:int}")]
  public async Task<IActionResult> DeleteDepartment([FromRoute] int id) => (await _service.DeleteDepartment(id)).Send();

  [HttpGet("rooms")]
  public async Task<IActionResult> GetRooms([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure) return reqResult.Send();
    var result = await _service.GetRooms(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("rooms/{id:int}")]
  public async Task<IActionResult> GetRoom([FromRoute] int id) => (await _service.GetRoom(id)).Send();

  [HttpPost("rooms")]
  public async Task<IActionResult> CreateRoom([FromBody] RoomRequest request) => (await _service.CreateRoom(request)).Send();

  [HttpPut("rooms/{id:int}")]
  public async Task<IActionResult> UpdateRoom([FromRoute] int id, [FromBody] RoomRequest request) => (await _service.UpdateRoom(id, request)).Send();

  [HttpDelete("rooms/{id:int}")]
  public async Task<IActionResult> DeleteRoom([FromRoute] int id) => (await _service.DeleteRoom(id)).Send();

  [HttpGet("projects")]
  public async Task<IActionResult> GetProjects([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure) return reqResult.Send();
    var result = await _service.GetProjects(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("projects/{id:int}")]
  public async Task<IActionResult> GetProject([FromRoute] int id) => (await _service.GetProject(id)).Send();

  [HttpPost("projects")]
  public async Task<IActionResult> CreateProject([FromBody] ProjectRequest request) => (await _service.CreateProject(request)).Send();

  [HttpPut("projects/{id:int}")]
  public async Task<IActionResult> UpdateProject([FromRoute] int id, [FromBody] ProjectRequest request) => (await _service.UpdateProject(id, request)).Send();

  [HttpDelete("projects/{id:int}")]
  public async Task<IActionResult> DeleteProject([FromRoute] int id) => (await _service.DeleteProject(id)).Send();

  [HttpGet("organizations")]
  public async Task<IActionResult> GetOrganizations([FromQuery] InelicomQuery query)
  {
    var reqResult = query.Validate();
    if (reqResult.IsFailure) return reqResult.Send();
    var result = await _service.GetOrganizations(reqResult.Value);
    return result.Send(query.Limit, query.PageIndex);
  }

  [HttpGet("organizations/{id:int}")]
  public async Task<IActionResult> GetOrganization([FromRoute] int id) => (await _service.GetOrganization(id)).Send();

  [HttpPost("organizations")]
  public async Task<IActionResult> CreateOrganization([FromBody] OrganizationRequest request) => (await _service.CreateOrganization(request)).Send();

  [HttpPut("organizations/{id:int}")]
  public async Task<IActionResult> UpdateOrganization([FromRoute] int id, [FromBody] OrganizationRequest request) => (await _service.UpdateOrganization(id, request)).Send();

  [HttpDelete("organizations/{id:int}")]
  public async Task<IActionResult> DeleteOrganization([FromRoute] int id) => (await _service.DeleteOrganization(id)).Send();
}
