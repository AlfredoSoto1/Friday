using Dapper;
using Friday.Backend.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
public sealed class SystemController : ControllerBase
{
  private readonly IDbConnectionFactory _dbFactory;

  public SystemController(IDbConnectionFactory dbFactory)
  {
    _dbFactory = dbFactory;
  }

  [HttpGet("api/status")]
  [HttpGet("api/v1/status")]
  public async Task<IActionResult> GetStatus()
  {
    try
    {
      using var connection = _dbFactory.Create();
      await connection.ExecuteScalarAsync<int>("SELECT 1;");

      return Result<BackendStatus, AppError>.Ok(new BackendStatus
      {
        Status = "ok",
        Database = "connected",
        CheckedAt = DateTimeOffset.UtcNow
      }).Send();
    }
    catch (Exception ex)
    {
      return Result<BackendStatus, AppError>.Ok(new BackendStatus
      {
        Status = "degraded",
        Database = $"disconnected: {ex.Message}",
        CheckedAt = DateTimeOffset.UtcNow
      }).Send();
    }
  }

  [HttpGet("api/catalog/summary")]
  [HttpGet("api/v1/catalog/summary")]
  public async Task<IActionResult> GetCatalogSummary()
  {
    try
    {
      using var connection = _dbFactory.Create();
      const string tablesSql = @"
        SELECT table_schema, table_name
          FROM information_schema.tables
        WHERE table_schema IN ('discord', 'inelicom')
          AND table_type = 'BASE TABLE'
        ORDER BY table_schema, table_name;
      ";

      var tables = (await connection.QueryAsync(tablesSql)).ToArray();
      var summaries = new List<TableSummary>();

      foreach (var table in tables)
      {
        var schema = (string)table.table_schema;
        var name = (string)table.table_name;
        var quotedSchema = schema.Replace("\"", "\"\"");
        var quotedName = name.Replace("\"", "\"\"");
        var rows = await connection.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM \"{quotedSchema}\".\"{quotedName}\";");

        summaries.Add(new TableSummary
        {
          Schema = schema,
          Table = name,
          Rows = rows
        });
      }

      var catalog = new CatalogSummary
      {
        Schemas = summaries
          .GroupBy(table => table.Schema)
          .Select(group => new SchemaSummary
          {
            Name = group.Key,
            Tables = group.ToArray()
          })
          .ToArray()
      };

      return Result<CatalogSummary, AppError>.Ok(catalog).Send();
    }
    catch (Exception ex)
    {
      return Result<CatalogSummary, AppError>.Fail(AppError.BadRequest(ex.Message)).Send();
    }
  }
}
