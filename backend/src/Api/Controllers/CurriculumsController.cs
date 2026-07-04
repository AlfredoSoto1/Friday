using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/inelicom/curriculums")]
[Route("api/v1/inelicom/curriculums")]
public sealed class CurriculumsController : ControllerBase
{
  private const long MaxFileSizeBytes = 20 * 1024 * 1024;

  private readonly IInelicomService _service;

  public CurriculumsController(IInelicomService service)
  {
    _service = service;
  }

  [HttpGet]
  public async Task<IActionResult> GetCurriculums()
  {
    var result = await _service.GetCurriculums();
    return result.Send(100, 0);
  }

  [HttpGet("{program}/file")]
  public async Task<IActionResult> GetCurriculumFile([FromRoute] string program)
  {
    var result = await _service.GetCurriculumFile(program);
    if (result.IsFailure)
    {
      return NotFound(new { message = result.Error.Message });
    }

    var file = result.Value;
    return File(file.Data, file.ContentType, file.FileName);
  }

  [HttpPut("{program}")]
  [RequestSizeLimit(MaxFileSizeBytes)]
  public async Task<IActionResult> UploadCurriculum([FromRoute] string program, IFormFile? file)
  {
    if (file is null || file.Length == 0)
    {
      return Result<Curriculum, AppError>.Fail(AppError.BadRequest("No file was uploaded.")).Send();
    }

    using var stream = new MemoryStream();
    await file.CopyToAsync(stream);

    var result = await _service.UploadCurriculum(program, file.FileName, file.ContentType, stream.ToArray());
    return result.Send();
  }

  [HttpDelete("{program}")]
  public async Task<IActionResult> DeleteCurriculum([FromRoute] string program)
  {
    var result = await _service.DeleteCurriculum(program);
    return result.Send();
  }
}
