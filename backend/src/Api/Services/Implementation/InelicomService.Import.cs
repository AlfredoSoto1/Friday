using System.Text;
using System.Text.Json;
using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<CsvImportResult, AppError>> ImportCsv(
    string kind,
    string fileName,
    Stream csvStream,
    bool append)
  {
    var normalizedKind = kind.Trim().ToLowerInvariant() switch
    {
      "building" or "buildings" or "googlepin" or "googlepins" or "google-pin" or "google-pins" => "buildings",
      "contact" or "contacts" => "contacts",
      "faculty" or "faculties" => "faculties",
      "project" or "projects" => "projects",
      "organization" or "organizations" or "organisation" or "organisations" => "organizations",
      _ => null
    };

    if (normalizedKind is null)
    {
      return Result<CsvImportResult, AppError>.Fail(
        AppError.ValidationFailed(
          "Unsupported CSV type. Use buildings, faculties, contacts, projects, or organizations."));
    }

    List<Dictionary<string, string?>> rows;
    try
    {
      using var reader = new StreamReader(csvStream);
      rows = ParseCsv(await reader.ReadToEndAsync());
    }
    catch (Exception ex)
    {
      return Result<CsvImportResult, AppError>.Fail(
        AppError.BadRequest($"Unable to read CSV: {ex.Message}"));
    }

    if (rows.Count == 0)
    {
      return Result<CsvImportResult, AppError>.Fail(
        AppError.ValidationFailed("CSV file is empty."));
    }

    var headers = rows[0].Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
    var requiredHeaders = normalizedKind switch
    {
      "buildings" => new[] { "code", "name", "gpin" },
      "contacts" => new[] { "name", "description", "email", "phone", "website", "services" },
      "faculties" => new[]
        { "ext", "web", "phone", "facebook", "email", "office", "name", "job_entitlement", "description", "instagram" },
      "departments" => new[] { "name" },
      "projects" => new[] { "web", "facebook", "instagram", "email", "name", "description" },
      "organizations" => new[] { "name", "description", "email", "facebook", "instagram", "twitter_x", "web" },
      _ => Array.Empty<string>()
    };

    var missingHeaders = requiredHeaders
      .Where(header => !headers.Contains(header))
      .ToArray();

    if (normalizedKind == "faculties"
      && !headers.Contains("abreviation")
      && !headers.Contains("abbreviation"))
    {
      missingHeaders = missingHeaders.Append("abreviation").ToArray();
    }

    if (normalizedKind == "contacts"
      && !headers.Contains("website")
      && headers.Contains("web"))
    {
      missingHeaders = missingHeaders.Where(header => header != "website").ToArray();
    }

    if (normalizedKind == "departments"
      && !headers.Contains("faculty_id")
      && !headers.Contains("faculty_name")
      && !headers.Contains("faculty"))
    {
      missingHeaders = missingHeaders.Append("faculty_id or faculty_name").ToArray();
    }

    if (normalizedKind == "departments"
      && !headers.Contains("building_id")
      && !headers.Contains("building_name")
      && !headers.Contains("building")
      && !headers.Contains("building_code"))
    {
      missingHeaders = missingHeaders.Append("building_id or building_name").ToArray();
    }

    if (missingHeaders.Length > 0)
    {
      return Result<CsvImportResult, AppError>.Fail(
        AppError.ValidationFailed(
          $"CSV is missing required column(s): {string.Join(", ", missingHeaders)}."));
    }

    try
    {
      using var connection = (NpgsqlConnection)_dbFactory.Create();
      var rowsJson = JsonSerializer.Serialize(rows);
      var importResult = await TransactionResult<CsvImportStats, AppError>
        .Begin(connection, exception => AppError.BadRequest(exception.Message))
        .AndThen((conn, transaction) =>
          _repository.ImportCsv(conn, transaction, normalizedKind, rowsJson, append))
        .Complete();

      return importResult.Transform(stats => new CsvImportResult
      {
        Kind = normalizedKind,
        FileName = fileName,
        Inserted = stats.Inserted,
        Updated = stats.Updated,
        Skipped = stats.Skipped,
        Errors = stats.Errors
      });
    }
    catch (Exception ex)
    {
      return Result<CsvImportResult, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private static List<Dictionary<string, string?>> ParseCsv(string csv)
  {
    var records = new List<List<string>>();
    var row = new List<string>();
    var field = new StringBuilder();
    var inQuotes = false;

    for (var i = 0; i < csv.Length; i++)
    {
      var current = csv[i];
      if (current == '"')
      {
        if (inQuotes && i + 1 < csv.Length && csv[i + 1] == '"')
        {
          field.Append('"');
          i++;
        }
        else
        {
          inQuotes = !inQuotes;
        }
      }
      else if (current == ',' && !inQuotes)
      {
        row.Add(field.ToString());
        field.Clear();
      }
      else if ((current == '\n' || current == '\r') && !inQuotes)
      {
        if (current == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
        {
          i++;
        }

        row.Add(field.ToString());
        field.Clear();
        if (row.Any(value => !string.IsNullOrWhiteSpace(value)))
        {
          records.Add(row);
        }

        row = new List<string>();
      }
      else
      {
        field.Append(current);
      }
    }

    if (field.Length > 0 || row.Count > 0)
    {
      row.Add(field.ToString());
      if (row.Any(value => !string.IsNullOrWhiteSpace(value)))
      {
        records.Add(row);
      }
    }

    if (records.Count == 0)
    {
      return new List<Dictionary<string, string?>>();
    }

    var headers = records[0]
      .Select(header => header.Trim().ToLowerInvariant())
      .ToArray();

    return records
      .Skip(1)
      .Select(record => headers
        .Select((header, index) => new { header, value = index < record.Count ? record[index] : null })
        .Where(entry => !string.IsNullOrWhiteSpace(entry.header))
        .ToDictionary(
          entry => entry.header,
          entry => string.IsNullOrWhiteSpace(entry.value) || entry.value.Trim() == "_"
            ? null
            : entry.value.Trim(),
          StringComparer.OrdinalIgnoreCase))
      .ToList();
  }
}
