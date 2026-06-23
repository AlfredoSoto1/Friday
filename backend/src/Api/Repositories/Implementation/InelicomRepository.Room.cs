using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<Paged<Room>, AppError>> GetRooms(IDbConnection connection, InelicomQuery query)
  {
    try
    {
      const string sql = @"
        SELECT room_id, code, name, building_id, department_id, created_at, COUNT(*) OVER() AS total
          FROM inelicom.rooms
        WHERE @Search IS NULL OR code ILIKE @Search OR name ILIKE @Search
        ORDER BY code
        LIMIT @Limit OFFSET @Offset;
      ";

      var records = await connection.QueryAsync(sql, new
      {
        Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{query.Search}%",
        query.Limit,
        Offset = query.PageIndex * query.Limit
      });

      var items = records.Select(MapToRoom).ToArray();
      var total = records.Select(record => (long)record.total).FirstOrDefault();
      return Result<Paged<Room>, AppError>.Ok(new Paged<Room>(items, total));
    }
    catch (Exception ex)
    {
      return Result<Paged<Room>, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Room, AppError>> GetRoom(IDbConnection connection, int id)
  {
    try
    {
      const string sql = @"
        SELECT room_id, code, name, building_id, department_id, created_at
          FROM inelicom.rooms
        WHERE room_id = @Id;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
      if (record is null)
      {
        return Result<Room, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Room, AppError>.Ok(MapToRoom(record));
    }
    catch (Exception ex)
    {
      return Result<Room, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Room, AppError>> CreateRoom(IDbConnection connection, IDbTransaction transaction, RoomRequest request)
  {
    try
    {
      const string sql = @"
        INSERT INTO inelicom.rooms (code, name, building_id, department_id)
        VALUES (@Code, @Name, @BuildingId, @DepartmentId)
        RETURNING room_id, code, name, building_id, department_id, created_at;
      ";

      var record = await connection.QuerySingleAsync(sql, request, transaction);
      return Result<Room, AppError>.Ok(MapToRoom(record));
    }
    catch (Exception ex)
    {
      return Result<Room, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<Room, AppError>> UpdateRoom(IDbConnection connection, IDbTransaction transaction, int id, RoomRequest request)
  {
    try
    {
      const string sql = @"
        UPDATE inelicom.rooms
           SET code = @Code, name = @Name, building_id = @BuildingId, department_id = @DepartmentId
        WHERE room_id = @Id
        RETURNING room_id, code, name, building_id, department_id, created_at;
      ";

      var record = await connection.QuerySingleOrDefaultAsync(sql, new
      {
        Id = id,
        request.Code,
        request.Name,
        request.BuildingId,
        request.DepartmentId
      }, transaction);
      if (record is null)
      {
        return Result<Room, AppError>.Fail(AppError.NotFound("Record not found."));
      }

      return Result<Room, AppError>.Ok(MapToRoom(record));
    }
    catch (Exception ex)
    {
      return Result<Room, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<bool, AppError>> DeleteRoom(IDbConnection connection, IDbTransaction transaction, int id)
  {
    try
    {
      const string sql = "DELETE FROM inelicom.rooms WHERE room_id = @Id;";
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

  private static Room MapToRoom(dynamic record) => new()
  {
    RoomId = (int)record.room_id,
    Code = (string)record.code,
    Name = (string)record.name,
    BuildingId = (int)record.building_id,
    DepartmentId = (int)record.department_id,
    CreatedAt = (DateTime)record.created_at
  };
}
