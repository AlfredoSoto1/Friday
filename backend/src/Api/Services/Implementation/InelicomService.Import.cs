using Friday.Backend.Api.Domain;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class InelicomService
{
  public async Task<Result<CsvImportResult, AppError>> ImportCsv(string kind, string fileName, Stream csvStream)
  {
    var normalizedKind = NormalizeKind(kind);
    if (normalizedKind is null)
    {
      return Result<CsvImportResult, AppError>.Fail(
        AppError.ValidationFailed("Unsupported CSV type. Use buildings, faculties, contacts, departments, projects, or organizations."));
    }

    List<Dictionary<string, string?>> rows;
    try
    {
      using var reader = new StreamReader(csvStream);
      rows = ParseCsv(await reader.ReadToEndAsync());
    }
    catch (Exception ex)
    {
      return Result<CsvImportResult, AppError>.Fail(AppError.BadRequest($"Unable to read CSV: {ex.Message}"));
    }

    if (rows.Count == 0)
    {
      return Result<CsvImportResult, AppError>.Fail(AppError.ValidationFailed("CSV file is empty."));
    }

    var headerResult = ValidateHeaders(normalizedKind, rows[0].Keys);
    if (headerResult.IsFailure)
    {
      return Result<CsvImportResult, AppError>.Fail(headerResult.Error);
    }

    var counter = new ImportCounter();
    var connection = (NpgsqlConnection)_dbFactory.Create();

    try
    {
      await connection.OpenAsync();
      await using var transaction = await connection.BeginTransactionAsync();

      for (var index = 0; index < rows.Count; index++)
      {
        var rowNumber = index + 2;
        var row = rows[index];
        var result = normalizedKind switch
        {
          "buildings" => await ImportBuilding(connection, transaction, row, rowNumber, counter),
          "contacts" => await ImportContact(connection, transaction, row, rowNumber, counter),
          "faculties" => await ImportFaculty(connection, transaction, row, rowNumber, counter),
          "departments" => await ImportDepartment(connection, transaction, row, rowNumber, counter),
          "projects" => await ImportProject(connection, transaction, row, rowNumber, counter),
          "organizations" => await ImportOrganization(connection, transaction, row, rowNumber, counter),
          _ => Result<bool, AppError>.Fail(AppError.ValidationFailed("Unsupported CSV type."))
        };

        if (result.IsFailure)
        {
          await transaction.RollbackAsync();
          return Result<CsvImportResult, AppError>.Fail(result.Error);
        }
      }

      await transaction.CommitAsync();
      return Result<CsvImportResult, AppError>.Ok(new CsvImportResult
      {
        Kind = normalizedKind,
        FileName = fileName,
        Inserted = counter.Inserted,
        Updated = counter.Updated,
        Skipped = counter.Skipped,
        Errors = counter.Errors
      });
    }
    catch (Exception ex)
    {
      return Result<CsvImportResult, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  private async Task<Result<bool, AppError>> ImportBuilding(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    Dictionary<string, string?> row,
    int rowNumber,
    ImportCounter counter)
  {
    var name = Value(row, "name");
    var gpin = Value(row, "gpin");
    if (name is null || gpin is null)
    {
      counter.AddSkipped($"Row {rowNumber}: building name and gpin are required.");
      return Result<bool, AppError>.Ok(false);
    }

    return await _repository.UpsertBuilding(connection, transaction, new BuildingRequest
    {
      Code = Value(row, "code"),
      Name = name,
      Gpin = gpin
    }, counter);
  }

  private async Task<Result<bool, AppError>> ImportContact(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    Dictionary<string, string?> row,
    int rowNumber,
    ImportCounter counter)
  {
    var name = Value(row, "name");
    var email = Value(row, "email");
    var phone = Value(row, "phone");
    var website = Value(row, "website") ?? Value(row, "web");
    if (name is null || email is null || phone is null || website is null)
    {
      counter.AddSkipped($"Row {rowNumber}: contact name, email, phone, and website are required.");
      return Result<bool, AppError>.Ok(false);
    }

    return await _repository.UpsertContact(connection, transaction, new ContactRequest
    {
      Name = name,
      Email = email,
      Phone = phone,
      Website = website
    }, counter);
  }

  private async Task<Result<bool, AppError>> ImportFaculty(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    Dictionary<string, string?> row,
    int rowNumber,
    ImportCounter counter)
  {
    var name = Value(row, "name");
    if (name is null)
    {
      counter.AddSkipped($"Row {rowNumber}: faculty name is required.");
      return Result<bool, AppError>.Ok(false);
    }

    return await _repository.UpsertFaculty(connection, transaction, new FacultyRequest
    {
      Extension = Value(row, "ext"),
      Web = Value(row, "web"),
      Phone = Value(row, "phone"),
      Facebook = Value(row, "facebook"),
      Email = Value(row, "email"),
      Office = Value(row, "office"),
      Name = name,
      JobEntitlement = Value(row, "job_entitlement"),
      Description = Value(row, "description"),
      Abbreviation = Value(row, "abreviation") ?? Value(row, "abbreviation"),
      Instagram = Value(row, "instagram")
    }, counter);
  }

  private async Task<Result<bool, AppError>> ImportDepartment(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    Dictionary<string, string?> row,
    int rowNumber,
    ImportCounter counter)
  {
    var name = Value(row, "name");
    if (name is null)
    {
      counter.AddSkipped($"Row {rowNumber}: department name is required.");
      return Result<bool, AppError>.Ok(false);
    }

    var facultyId = ParseInt(Value(row, "faculty_id"));
    if (facultyId is null)
    {
      var facultyName = Value(row, "faculty_name") ?? Value(row, "faculty");
      if (facultyName is not null)
      {
        var facultyResult = await _repository.GetFacultyIdByName(connection, transaction, facultyName);
        if (facultyResult.IsFailure) return Result<bool, AppError>.Fail(facultyResult.Error);
        facultyId = facultyResult.Value;
      }
    }

    var buildingId = ParseInt(Value(row, "building_id"));
    if (buildingId is null)
    {
      var buildingName = Value(row, "building_name") ?? Value(row, "building") ?? Value(row, "building_code");
      if (buildingName is not null)
      {
        var buildingResult = await _repository.GetBuildingIdByNameOrCode(connection, transaction, buildingName);
        if (buildingResult.IsFailure) return Result<bool, AppError>.Fail(buildingResult.Error);
        buildingId = buildingResult.Value;
      }
    }

    if (facultyId is null || buildingId is null)
    {
      counter.AddSkipped($"Row {rowNumber}: department faculty and building are required and must match existing records.");
      return Result<bool, AppError>.Ok(false);
    }

    return await _repository.UpsertDepartment(connection, transaction, new DepartmentRequest
    {
      Name = name,
      FacultyId = facultyId.Value,
      BuildingId = buildingId.Value
    }, counter);
  }

  private async Task<Result<bool, AppError>> ImportProject(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    Dictionary<string, string?> row,
    int rowNumber,
    ImportCounter counter)
  {
    var name = Value(row, "name");
    var description = Value(row, "description");
    if (name is null || description is null)
    {
      counter.AddSkipped($"Row {rowNumber}: project name and description are required.");
      return Result<bool, AppError>.Ok(false);
    }

    return await _repository.UpsertProject(connection, transaction, new ProjectRequest
    {
      Web = Value(row, "web"),
      Facebook = Value(row, "facebook"),
      Instagram = Value(row, "instagram"),
      Email = Value(row, "email"),
      Name = name,
      Description = description
    }, counter);
  }

  private async Task<Result<bool, AppError>> ImportOrganization(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    Dictionary<string, string?> row,
    int rowNumber,
    ImportCounter counter)
  {
    var name = Value(row, "name");
    var description = Value(row, "description");
    if (name is null || description is null)
    {
      counter.AddSkipped($"Row {rowNumber}: organization name and description are required.");
      return Result<bool, AppError>.Ok(false);
    }

    return await _repository.UpsertOrganization(connection, transaction, new OrganizationRequest
    {
      Name = name,
      Description = description,
      Email = Value(row, "email"),
      Facebook = Value(row, "facebook"),
      Instagram = Value(row, "instagram"),
      TwitterX = Value(row, "twitter_x"),
      Web = Value(row, "web")
    }, counter);
  }

  private static string? NormalizeKind(string kind)
  {
    return kind.Trim().ToLowerInvariant() switch
    {
      "building" or "buildings" or "googlepin" or "googlepins" or "google-pin" or "google-pins" => "buildings",
      "contact" or "contacts" => "contacts",
      "faculty" or "faculties" => "faculties",
      "department" or "departments" => "departments",
      "project" or "projects" => "projects",
      "organization" or "organizations" or "organisation" or "organisations" => "organizations",
      _ => null
    };
  }

  private static Result<bool, AppError> ValidateHeaders(string kind, IEnumerable<string> headers)
  {
    var headerSet = headers.ToHashSet(StringComparer.OrdinalIgnoreCase);
    var required = kind switch
    {
      "buildings" => new[] { "code", "name", "gpin" },
      "contacts" => new[] { "name", "email", "phone", "website" },
      "faculties" => new[] { "ext", "web", "phone", "facebook", "email", "office", "name", "job_entitlement", "description", "instagram" },
      "departments" => new[] { "name" },
      "projects" => new[] { "web", "facebook", "instagram", "email", "name", "description" },
      "organizations" => new[] { "name", "description", "email", "facebook", "instagram", "twitter_x", "web" },
      _ => Array.Empty<string>()
    };

    var missing = required.Where(header => !headerSet.Contains(header)).ToArray();
    if (kind == "faculties" && !headerSet.Contains("abreviation") && !headerSet.Contains("abbreviation"))
    {
      missing = missing.Append("abreviation").ToArray();
    }

    if (kind == "contacts" && missing.Contains("website") && headerSet.Contains("web"))
    {
      missing = missing.Where(header => header != "website").ToArray();
    }

    if (kind == "departments"
      && !headerSet.Contains("faculty_id")
      && !headerSet.Contains("faculty_name")
      && !headerSet.Contains("faculty"))
    {
      missing = missing.Append("faculty_id or faculty_name").ToArray();
    }

    if (kind == "departments"
      && !headerSet.Contains("building_id")
      && !headerSet.Contains("building_name")
      && !headerSet.Contains("building")
      && !headerSet.Contains("building_code"))
    {
      missing = missing.Append("building_id or building_name").ToArray();
    }

    return missing.Length == 0
      ? Result<bool, AppError>.Ok(true)
      : Result<bool, AppError>.Fail(AppError.ValidationFailed($"CSV is missing required column(s): {string.Join(", ", missing)}."));
  }

  private static int? ParseInt(string? value)
  {
    return int.TryParse(value, out var parsed) ? parsed : null;
  }

  private static string? Value(Dictionary<string, string?> row, string key)
  {
    return row.TryGetValue(key, out var value) ? Clean(value) : null;
  }

  private static string? Clean(string? value)
  {
    if (string.IsNullOrWhiteSpace(value)) return null;

    var trimmed = value.Trim();
    return trimmed == "_" ? null : trimmed;
  }

  private static List<Dictionary<string, string?>> ParseCsv(string csv)
  {
    var records = new List<List<string>>();
    var row = new List<string>();
    var field = new System.Text.StringBuilder();
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
        if (current == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
        row.Add(field.ToString());
        field.Clear();
        if (row.Any(value => !string.IsNullOrWhiteSpace(value))) records.Add(row);
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
      if (row.Any(value => !string.IsNullOrWhiteSpace(value))) records.Add(row);
    }

    if (records.Count == 0) return new List<Dictionary<string, string?>>();

    var headers = records[0]
      .Select(header => header.Trim())
      .ToArray();

    return records
      .Skip(1)
      .Select(record => headers
        .Select((header, index) => new { header, value = index < record.Count ? record[index] : null })
        .Where(entry => !string.IsNullOrWhiteSpace(entry.header))
        .ToDictionary(entry => entry.header, entry => entry.value, StringComparer.OrdinalIgnoreCase))
      .ToList();
  }
}
