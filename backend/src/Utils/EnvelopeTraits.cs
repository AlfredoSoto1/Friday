using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Utils;

public static class EnvelopeTraits
{
  public static IActionResult Send<T>(this Result<T, AppError> result, MetaEnvelope? meta = null)
  {
    return result.IsSuccess
      ? new OkObjectResult(Success(result.Value, meta))
      : result.Error.ToEnvelopeActionResult<T>();
  }

  public static IActionResult Send<T>(this Result<Paged<T>, AppError> result, int limit, int pageIndex)
  {
    return result.IsSuccess
      ? new OkObjectResult(SuccessList(result.Value.Items, limit, pageIndex, result.Value.Total))
      : result.Error.ToEnvelopeActionResult<IEnumerable<T>>();
  }

  public static IActionResult Send<T, M>(this Result<PagedExt<T, M>, AppError> result, int limit, int pageIndex)
  {
    return result.IsSuccess
      ? new OkObjectResult(SuccessList(result.Value.Items, result.Value.Meta, limit, pageIndex, result.Value.Total))
      : result.Error.ToEnvelopeActionResult<IEnumerable<T>>();
  }

  private static ApiResponse<T> Success<T>(T data, MetaEnvelope? meta = null)
  {
    return new ApiResponse<T>
    {
      Status = "success",
      Data = data,
      Meta = BuildMeta(meta),
      Error = null
    };
  }

  private static ApiResponse<IEnumerable<T>> SuccessList<T>(IEnumerable<T> data, int limit, int pageIndex, long total)
  {
    var remaining = total - (long)limit * (pageIndex + 1L);
    if (remaining < 0)
    {
      remaining = 0;
    }

    return new ApiResponse<IEnumerable<T>>
    {
      Status = "success",
      Data = data,
      Meta = BuildMeta(new MetaEnvelope
      {
        Limit = limit,
        PageIndex = pageIndex,
        Total = total,
        Remaining = remaining
      }),
      Error = null
    };
  }

  private static ApiResponse<IEnumerable<T>> SuccessList<T, M>(IEnumerable<T> data, M meta, int limit, int pageIndex, long total)
  {
    var remaining = total - (long)limit * (pageIndex + 1L);
    if (remaining < 0)
    {
      remaining = 0;
    }

    return new ApiResponse<IEnumerable<T>>
    {
      Status = "success",
      Data = data,
      Meta = BuildMeta(new MetaEnvelope
      {
        Limit = limit,
        PageIndex = pageIndex,
        Total = total,
        Remaining = remaining,
        Custom = JsonSerializer.SerializeToElement(meta)
      }),
      Error = null
    };
  }

  private static MetaEnvelope BuildMeta(MetaEnvelope? meta = null)
  {
    meta ??= new MetaEnvelope();
    meta.Timestamp ??= DateTimeOffset.UtcNow;
    meta.RequestId ??= Activity.Current?.Id;

    var elapsed = RequestTiming.ElapsedMilliseconds;
    if (elapsed.HasValue)
    {
      meta.ProcessingTimeMs ??= elapsed.Value;
    }

    return meta;
  }

  private static IActionResult ToEnvelopeActionResult<T>(this AppError error)
  {
    var (statusCode, message) = MapError(error);
    var payload = new ApiResponse<T>
    {
      Status = "error",
      Data = default,
      Error = new ErrorInfo
      {
        Code = statusCode,
        Message = message
      },
      Meta = BuildMeta()
    };

    return new ObjectResult(payload) { StatusCode = statusCode };
  }

  private static (int statusCode, string message) MapError(AppError error)
  {
    var code = error.Kind switch
    {
      AppErrorKind.NotFound => StatusCodes.Status404NotFound,
      AppErrorKind.BadRequest => StatusCodes.Status400BadRequest,
      AppErrorKind.ValidationFailed => StatusCodes.Status422UnprocessableEntity,
      AppErrorKind.Unauthorized => StatusCodes.Status403Forbidden,
      AppErrorKind.Conflict => StatusCodes.Status409Conflict,
      AppErrorKind.ServerError => StatusCodes.Status500InternalServerError,
      AppErrorKind.Fallback => StatusCodes.Status500InternalServerError,
      _ => StatusCodes.Status500InternalServerError
    };

    return (code, error.Message);
  }
}
