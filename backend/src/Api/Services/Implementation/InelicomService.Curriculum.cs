using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  private static readonly string[] KnownPrograms = ["INEL", "ICOM"];

  public async Task<Result<Paged<Curriculum>, AppError>> GetCurriculums()
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetCurriculums(connection);
  }

  public async Task<Result<CurriculumFile, AppError>> GetCurriculumFile(string program)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetCurriculumFile(connection, NormalizeProgram(program));
  }

  public async Task<Result<Curriculum, AppError>> UploadCurriculum(
    string program,
    string fileName,
    string contentType,
    byte[] fileData)
  {
    var normalizedProgram = NormalizeProgram(program);
    if (!KnownPrograms.Contains(normalizedProgram))
    {
      return Result<Curriculum, AppError>.Fail(
        AppError.BadRequest($"Unknown program '{program}'. Expected one of: {string.Join(", ", KnownPrograms)}."));
    }

    if (contentType != "application/pdf")
    {
      return Result<Curriculum, AppError>.Fail(AppError.BadRequest("Only PDF files are accepted."));
    }

    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<Curriculum, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.UpsertCurriculum(conn, tran, normalizedProgram, fileName, contentType, fileData))
      .Complete();
  }

  public async Task<Result<bool, AppError>> DeleteCurriculum(string program)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<bool, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.DeleteCurriculum(conn, tran, NormalizeProgram(program)))
      .Complete();
  }

  private static string NormalizeProgram(string program) => program.Trim().ToUpperInvariant();
}
