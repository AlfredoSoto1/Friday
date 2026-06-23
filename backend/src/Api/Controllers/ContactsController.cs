using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/contacts")]
[Route("api/v1/inelicom/contacts")]
public sealed class ContactsController : ControllerBase
{
  private readonly IInelicomService _service;

  public ContactsController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet]
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

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetContact([FromRoute] int id)
  {
    var result = await _service.GetContact(id);
    return result.Send();
  }

  [HttpPost]
  public async Task<IActionResult> CreateContact([FromBody] ContactRequest request)
  {
    var result = await _service.CreateContact(request);
    return result.Send();
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> UpdateContact([FromRoute] int id, [FromBody] ContactRequest request)
  {
    var result = await _service.UpdateContact(id, request);
    return result.Send();
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteContact([FromRoute] int id)
  {
    var result = await _service.DeleteContact(id);
    return result.Send();
  }
}
