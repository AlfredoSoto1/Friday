using Npgsql;
using System.Data;

namespace Utils;

public readonly struct TransactionResult<T, E>
{
  private readonly NpgsqlConnection _conn;
  private readonly NpgsqlTransaction _tran;
  private readonly Task<Result<T, E>> _task;
  private readonly Func<Exception, E> _exMapper;

  private TransactionResult(NpgsqlConnection conn, NpgsqlTransaction tx, Task<Result<T, E>> task, Func<Exception, E> exMapper)
  {
    _task = task;
    _conn = conn;
    _tran = tx;
    _exMapper = exMapper;
  }

  public Task<Result<T, E>> Value => _task;

  // ==========================================================================
  //  Begins a new transaction
  // ==========================================================================
  public static TransactionResult<T, E> Begin(NpgsqlConnection conn, Func<Exception, E> exMapper)
  {
    conn.Open();
    var tran = conn.BeginTransaction();
    var seed = Task.FromResult(Result<T, E>.Ok(default!));
    return new TransactionResult<T, E>(
        conn,
        tran,
        seed,
        exMapper
    );
  }

  // ==========================================================================
  //  Completes a transaction by:
  //  + Commiting the changes
  //  + Rolling-back the changes that were done
  //  + Closes the DB connection after everything
  // ==========================================================================
  public async Task<Result<T, E>> Complete()
  {
    var result = await _task.ConfigureAwait(false);

    if (result.IsSuccess)
      await _tran.CommitAsync();
    else
      await _tran.RollbackAsync();

    await _conn.CloseAsync();

    return result;
  }

  // ==========================================================================
  //  Expects a result after curent's transaction-step is completed
  //  successfully. If it fails, it returns a ResponseError as part
  //  of the result.
  // ==========================================================================
  public TransactionResult<U, E> AndThen<U>(Func<IDbConnection, IDbTransaction, Task<Result<U, E>>> f)
  {
    var conn = _conn;
    var tran = _tran;
    var prevTask = _task;
    var exMapper = _exMapper;

    async Task<Result<U, E>> ChainAsync()
    {
      try
      {
        var prev = await prevTask.ConfigureAwait(false);
        if (prev.IsFailure)
          return Result<U, E>.Fail(prev.Error);

        return await f(conn, tran).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        return Result<U, E>.Fail(exMapper(ex));
      }
    }

    return new TransactionResult<U, E>(
        conn,
        tran,
        ChainAsync(),
        exMapper
    );
  }

  // ==========================================================================
  //  Expects a result after curent's transaction-step is completed
  //  successfully. If it fails, it returns a ResponseError as part
  //  of the result.
  //
  //  + Provides as input for next DB access, the previous successful result.
  // ==========================================================================
  public TransactionResult<U, E> AndThen<U>(Func<IDbConnection, IDbTransaction, T, Task<Result<U, E>>> f)
  {
    var conn = _conn;
    var tran = _tran;
    var prevTask = _task;
    var exMapper = _exMapper;

    async Task<Result<U, E>> ChainAsync()
    {
      try
      {
        var prev = await prevTask.ConfigureAwait(false);
        if (prev.IsFailure)
          return Result<U, E>.Fail(prev.Error);

        return await f(conn, tran, prev.Value).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        return Result<U, E>.Fail(exMapper(ex));
      }
    }

    return new TransactionResult<U, E>(
        conn,
        tran,
        ChainAsync(),
        exMapper
    );
  }

  // ==========================================================================
  //  If the current transaction produced a ResponseError, it will
  //  trigger the next in chain capturing the result and do something with it.
  // ==========================================================================
  public TransactionResult<T, E> OrElse(Func<E, Result<T, E>> f)
  {
    var conn = _conn;
    var tran = _tran;
    var prevTask = _task;
    var exMapper = _exMapper;

    async Task<Result<T, E>> ChainAsync()
    {
      var prev = await prevTask.ConfigureAwait(false);
      if (prev.IsSuccess)
        return prev;

      return f(prev.Error);
    }

    return new TransactionResult<T, E>(
        conn,
        tran,
        ChainAsync(),
        exMapper
    );
  }

  // ==========================================================================
  //  If the current transaction produced a ResponseError, it will
  //  trigger the next in chain capturing the result and do something with it.
  //  
  //  *This keeps the transaction alive.
  // ==========================================================================
  public TransactionResult<T, E> OrElse(Func<IDbConnection, IDbTransaction, Task<Result<T, E>>> f)
  {
    var conn = _conn;
    var tran = _tran;
    var prevTask = _task;
    var exMapper = _exMapper;

    async Task<Result<T, E>> ChainAsync()
    {
      try
      {
        var prev = await prevTask.ConfigureAwait(false);
        if (prev.IsSuccess)
          return prev;

        return await f(conn, tran).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        return Result<T, E>.Fail(exMapper(ex));
      }
    }

    return new TransactionResult<T, E>(
        conn,
        tran,
        ChainAsync(),
        exMapper
    );
  }
}