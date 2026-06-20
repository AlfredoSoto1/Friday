namespace Utils;

public readonly struct Result<T, E>
{
  private readonly T _value;
  private readonly E _error;

  public bool IsSuccess { get; }
  public bool IsFailure => !IsSuccess;

  private Result(T value)
  {
    _value = value;
    _error = default!;
    IsSuccess = true;
  }

  private Result(E error)
  {
    _error = error;
    _value = default!;
    IsSuccess = false;
  }

  public T Value => IsSuccess
    ? _value
    : throw new InvalidOperationException("No value present");

  public E Error => IsFailure
    ? _error
    : throw new InvalidOperationException("No error present");

  public static Result<T, E> Ok(T value) => new(value);
  public static Result<T, E> Fail(E error) => new(error);

  public Result<U, E> AndThen<U>(Func<T, Result<U, E>> f)
    => IsSuccess ? f(_value) : Result<U, E>.Fail(_error);

  public Result<T, F> OrElse<F>(Func<E, Result<T, F>> f)
    => IsFailure ? f(_error) : Result<T, F>.Ok(_value);

  public Result<U, E> Transform<U>(Func<T, U> f)
    => IsSuccess ? Result<U, E>.Ok(f(_value)) : Result<U, E>.Fail(_error);
}
