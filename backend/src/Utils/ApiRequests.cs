namespace Utils;

public class BaseUrlQuery
{
  public int Limit { get; init; } = 100;
  public int PageIndex { get; init; } = 0;
  public bool IncludeTotal { get; init; } = true;
}

public record PageOptions
{
  public int Limit { get; init; }
  public int PageIndex { get; init; }
  public bool IncludeTotal { get; init; }

  public PageOptions(int limit, int pageIndex, bool includeTotal)
  {
    Limit = limit;
    PageIndex = pageIndex;
    IncludeTotal = includeTotal;
  }
}

public static class ApiRequestTrait
{
  public static Result<T, AppError> Validate<T>(this T req) where T : BaseUrlQuery
  {
    if (req.Limit < 0 || req.PageIndex < 0)
    {
      return Result<T, AppError>
        .Fail(AppError.BadRequest("Cannot set limit or pageIndex to less than zero."));
    }

    return Result<T, AppError>.Ok(req);
  }
}
