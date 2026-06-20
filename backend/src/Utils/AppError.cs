namespace Utils;

public enum AppErrorKind
{
  NotFound,
  Fallback,            // e.g. fallback for unexpected errors (like a default case in a switch)  
  ServerError,         // e.g. unexpected exception  
  BadRequest,          // e.g. missing body
  ValidationFailed,    // e.g. DTO-level validation
  Unauthorized,        // e.g. user can’t edit this quote
  Conflict,            // e.g. quote already locked/in-use
  Unknown              // fallback for unexpected errors
}

public readonly record struct AppError(AppErrorKind Kind, string Message)
{
  public override string ToString() => $"{Kind}: {Message}";

  public static AppError NotFound(string message) => new AppError(AppErrorKind.NotFound, message);
  public static AppError ServerError(string message) => new AppError(AppErrorKind.ServerError, message);
  public static AppError BadRequest(string message) => new AppError(AppErrorKind.BadRequest, message);
  public static AppError ValidationFailed(string message) => new AppError(AppErrorKind.ValidationFailed, message);
  public static AppError Unauthorized(string message) => new AppError(AppErrorKind.Unauthorized, message);
  public static AppError Conflict(string message) => new AppError(AppErrorKind.Conflict, message);
  public static AppError Unknown(string message) => new AppError(AppErrorKind.Unknown, message);
  public static AppError Fallback(string message) => new AppError(AppErrorKind.Fallback, message);
}