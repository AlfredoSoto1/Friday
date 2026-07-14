using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/imports")]
[Route("api/v1/inelicom/imports")]
public sealed class InelicomImportsController : ControllerBase
{
  private readonly IInelicomService _service;

  public InelicomImportsController(IInelicomService service)
  {
    _service = service;
  }

  [HttpPost("{kind}")]
  [RequestSizeLimit(10_000_000)]
  public async Task<IActionResult> ImportCsv(
    [FromRoute] string kind,
    [FromForm] IFormFile? file,
    [FromForm] bool append = false)
  {
    if (file is null || file.Length == 0)
    {
      return Result<bool, AppError>
        .Fail(AppError.ValidationFailed("Upload a non-empty CSV file."))
        .Send();
    }

    if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
    {
      return Result<bool, AppError>
        .Fail(AppError.ValidationFailed("Only CSV files are supported."))
        .Send();
    }

    await using var stream = file.OpenReadStream();
    var result = await _service.ImportCsv(kind, file.FileName, stream, append);
    return result.Send();
  }
}
