using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<Paged<Curriculum>, AppError>> GetCurriculums(IDbConnection connection)
  {
    try
    {
      const string sql = @"
        SELECT curriculum_id, program, file_name, content_type, length(file_data) AS file_size, uploaded_at
          FROM inelicom.curriculums
        ORDER BY program;
      ";

      var records = await connection.QueryAsync(sql);
      var items = records.Select(MapToCurriculum).ToArray();
      return Result<Paged<Curriculum>, AppError>.Ok(new Paged<Curriculum>(items, items.Length));
    }
    catch (Exception ex)
    {
      return Result<Paged<Curriculum>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<CurriculumFile, AppError>> GetCurriculumFile(IDbConnection connection, string program)
  {
    try
    {
      const string sql = @"
        SELECT file_name, content_type, file_data
          FROM inelicom.curriculums
        WHERE program = @Program;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Program = program });
      if (record is null)
      {
        return Result<CurriculumFile, AppError>.Fail(AppError.NotFound("No curriculum uploaded for this program."));
      }

      return Result<CurriculumFile, AppError>.Ok(new CurriculumFile(
        (string)record.file_name,
        (string)record.content_type,
        (byte[])record.file_data
      ));
    }
    catch (Exception ex)
    {
      return Result<CurriculumFile, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Curriculum, AppError>> UpsertCurriculum(
    IDbConnection connection,
    IDbTransaction transaction,
    string program,
    string fileName,
    string contentType,
    byte[] fileData)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.curriculums (program, file_name, content_type, file_data)
        VALUES (@Program, @FileName, @ContentType, @FileData)
        ON CONFLICT (program) DO UPDATE
          SET file_name    = EXCLUDED.file_name,
              content_type = EXCLUDED.content_type,
              file_data    = EXCLUDED.file_data,
              uploaded_at  = CURRENT_TIMESTAMP
        RETURNING curriculum_id, program, file_name, content_type, length(file_data) AS file_size, uploaded_at;
      ";

      var record = await connection.QuerySingleAsync(sql, new
      {
        Program = program,
        FileName = fileName,
        ContentType = contentType,
        FileData = fileData
      }, transaction);

      return Result<Curriculum, AppError>.Ok(MapToCurriculum(record));
    }
    catch (Exception ex)
    {
      return Result<Curriculum, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteCurriculum(IDbConnection connection, IDbTransaction transaction, string program)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.curriculums WHERE program = @Program;";
      var affected = await connection.ExecuteAsync(sql, new { Program = program }, transaction);
      if (affected == 0)
      {
        return Result<bool, AppError>.Fail(AppError.NotFound("No curriculum uploaded for this program."));
      }

      return Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static Curriculum MapToCurriculum(dynamic record) => new()
  {
    CurriculumId = (int)record.curriculum_id,
    Program = (string)record.program,
    FileName = (string)record.file_name,
    ContentType = (string)record.content_type,
    FileSize = (long)record.file_size,
    UploadedAt = (DateTime)record.uploaded_at
  };
}
