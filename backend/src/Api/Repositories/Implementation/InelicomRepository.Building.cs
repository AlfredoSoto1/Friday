using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<Paged<Building>, AppError>> GetBuildings(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT building_id, code, name, gpin, created_at, COUNT(*) OVER() AS total
          FROM inelicom.buildings
        WHERE @Search IS NULL OR name ILIKE @Search OR code ILIKE @Search
        ORDER BY name
        LIMIT @Limit OFFSET @Offset;
      ";

      var records = await connection.QueryAsync(sql, new
      {
        Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{query.Search}%",
        query.Limit,
        Offset = query.PageIndex * query.Limit
      });

      var items = records.Select(MapToBuilding).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Building>, AppError>.Ok(new Paged<Building>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Building>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Building, AppError>> GetBuilding(IDbConnection connection, int id)
  {
    try
    {
      const string sql = @"
        SELECT building_id, code, name, gpin, created_at
          FROM inelicom.buildings
        WHERE building_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Building, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Building, AppError>.Ok(MapToBuilding(record));
    }
    catch (Exception ex)
    {
      return Result<Building, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Building, AppError>> CreateBuilding(IDbConnection connection, IDbTransaction transaction, BuildingRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.buildings (code, name, gpin)
        VALUES (@Code, @Name, @Gpin)
        RETURNING building_id, code, name, gpin, created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Building, AppError>.Ok(MapToBuilding(record));
    }
    catch (Exception ex)
    {
      return Result<Building, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Building, AppError>> UpdateBuilding(IDbConnection connection, IDbTransaction transaction, int id, BuildingRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE inelicom.buildings
           SET code = @Code, name = @Name, gpin = @Gpin
        WHERE building_id = @Id
        RETURNING building_id, code, name, gpin, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Code,
        request.Name,
        request.Gpin
      }, transaction);
      if (record is null)
      {
        return Result<Building, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Building, AppError>.Ok(MapToBuilding(record));
    }
    catch (Exception ex)
    {
      return Result<Building, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteBuilding(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.buildings WHERE building_id = @Id;";
      var affected = await connection.ExecuteAsync(sql, new { Id = id }, transaction);
      if (affected == 0)
      {
        return Result<bool, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<bool, AppError>.Ok(true);
    }
    catch (Exception ex)
    {
      return Result<bool, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static Building MapToBuilding(dynamic record) => new()
  {
    BuildingId = (int)record.building_id,
    Code = record.code as string,
    Name = (string)record.name,
    Gpin = (string)record.gpin,
    CreatedAt = (DateTime)record.created_at
  };
}
