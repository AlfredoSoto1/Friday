using System.Data;
using Dapper;
using Friday.Backend.Api.Domain;
using Utils;

namespace Friday.Backend.Api.Repositories;

public sealed partial class InelicomRepository
{
  public async Task<Result<CsvImportStats, AppError>> ImportCsv(
    IDbConnection connection,
    IDbTransaction transaction,
    string kind,
    string rowsJson,
    bool append)
  {
    const string contactsSql = @"
      WITH input AS (
        SELECT
          row_number() OVER ()::int AS source_row,
          name,
          description,
          email,
          phone,
          COALESCE(website, web) AS website,
          services
        FROM jsonb_to_recordset(CAST(@RowsJson AS jsonb)) AS source(
          name text,
          email text,
          phone text,
          description text,
          website text,
          web text,
          services text)
      ), valid AS (
        SELECT *
        FROM input
        WHERE name IS NOT NULL
          AND description IS NOT NULL
          AND email IS NOT NULL
          AND phone IS NOT NULL
          AND website IS NOT NULL
          AND services IS NOT NULL
      ), matched AS (
        SELECT DISTINCT ON (valid.source_row)
          valid.source_row,
          contacts.contact_id,
          valid.name,
          valid.description,
          valid.email,
          valid.phone,
          valid.website,
          valid.services
        FROM valid
        INNER JOIN inelicom.contacts contacts
          ON contacts.email = valid.email OR contacts.name = valid.name
        ORDER BY valid.source_row,
          (contacts.email = valid.email) DESC,
          contacts.contact_id
      ), updated AS (
        UPDATE inelicom.contacts contacts
        SET name = matched.name,
            description = matched.description,
            email = matched.email,
            phone = matched.phone,
            website = matched.website,
            services = matched.services
        FROM matched
        WHERE contacts.contact_id = matched.contact_id
          AND NOT @Append
        RETURNING contacts.contact_id
      ), inserted AS (
        INSERT INTO inelicom.contacts (name, description, email, phone, website, services)
        SELECT valid.name, valid.description, valid.email, valid.phone, valid.website, valid.services
        FROM valid
        WHERE NOT EXISTS (
          SELECT 1
          FROM matched
          WHERE matched.source_row = valid.source_row)
        RETURNING contact_id
      )
      SELECT
        (SELECT COUNT(*) FROM inserted)::int AS ""Inserted"",
        (SELECT COUNT(*) FROM updated)::int AS ""Updated"",
        (SELECT COUNT(*) FROM input
          WHERE name IS NULL
             OR description IS NULL
             OR email IS NULL
             OR phone IS NULL
             OR website IS NULL
             OR services IS NULL)::int
          + CASE WHEN @Append THEN (SELECT COUNT(*) FROM matched)::int ELSE 0 END AS ""Skipped"";
    ";

    const string buildingsSql = @"
      WITH input AS (
        SELECT row_number() OVER ()::int AS source_row, code, name, gpin
        FROM jsonb_to_recordset(CAST(@RowsJson AS jsonb)) AS source(
          code text,
          name text,
          gpin text)
      ), valid AS (
        SELECT *
        FROM input
        WHERE name IS NOT NULL AND gpin IS NOT NULL
      ), upsert AS (
        INSERT INTO inelicom.buildings (code, name, gpin)
        SELECT code, name, gpin
        FROM valid
        ON CONFLICT (code) DO UPDATE
          SET code = EXCLUDED.code,
              name = EXCLUDED.name,
              gpin = EXCLUDED.gpin
        WHERE NOT @Append
        RETURNING xmax = 0 AS inserted
      )
      SELECT
        COUNT(*) FILTER (WHERE inserted)::int AS ""Inserted"",
        COUNT(*) FILTER (WHERE NOT inserted)::int AS ""Updated"",
        (SELECT COUNT(*) FROM input
          WHERE name IS NULL OR gpin IS NULL)::int
          + CASE WHEN @Append THEN ((SELECT COUNT(*) FROM valid) - (SELECT COUNT(*) FROM upsert))::int ELSE 0 END AS ""Skipped""
      FROM upsert;
    ";

    const string facultiesSql = @"
      WITH input AS (
        SELECT
          row_number() OVER ()::int AS source_row,
          name,
          ext,
          web,
          phone,
          facebook,
          email,
          office,
          job_entitlement,
          description,
          COALESCE(abbreviation, abreviation) AS abbreviation,
          instagram
        FROM jsonb_to_recordset(CAST(@RowsJson AS jsonb)) AS source(
          name text,
          ext text,
          web text,
          phone text,
          facebook text,
          email text,
          office text,
          job_entitlement text,
          description text,
          abbreviation text,
          abreviation text,
          instagram text)
      ), valid AS (
        SELECT *
        FROM input
        WHERE name IS NOT NULL
      ), upsert AS (
        INSERT INTO inelicom.faculties
          (name, extension, web, phone, facebook, email, office,
           job_entitlement, description, abbreviation, instagram)
        SELECT
          name, ext, web, phone, facebook, email, office,
          job_entitlement, description, abbreviation, instagram
        FROM valid
        ON CONFLICT (name) DO UPDATE
          SET name = EXCLUDED.name,
              extension = EXCLUDED.extension,
              web = EXCLUDED.web,
              phone = EXCLUDED.phone,
              facebook = EXCLUDED.facebook,
              email = EXCLUDED.email,
              office = EXCLUDED.office,
              job_entitlement = EXCLUDED.job_entitlement,
              description = EXCLUDED.description,
              abbreviation = EXCLUDED.abbreviation,
              instagram = EXCLUDED.instagram
        WHERE NOT @Append
        RETURNING xmax = 0 AS inserted
      )
      SELECT
        COUNT(*) FILTER (WHERE inserted)::int AS ""Inserted"",
        COUNT(*) FILTER (WHERE NOT inserted)::int AS ""Updated"",
        (SELECT COUNT(*) FROM input WHERE name IS NULL)::int
          + CASE WHEN @Append THEN ((SELECT COUNT(*) FROM valid) - (SELECT COUNT(*) FROM upsert))::int ELSE 0 END AS ""Skipped""
      FROM upsert;
    ";

    const string projectsSql = @"
      WITH input AS (
        SELECT
          row_number() OVER ()::int AS source_row,
          web,
          facebook,
          instagram,
          email,
          name,
          description
        FROM jsonb_to_recordset(CAST(@RowsJson AS jsonb)) AS source(
          web text,
          facebook text,
          instagram text,
          email text,
          name text,
          description text)
      ), valid AS (
        SELECT *
        FROM input
        WHERE name IS NOT NULL AND description IS NOT NULL
      ), upsert AS (
        INSERT INTO inelicom.projects
          (web, facebook, instagram, email, name, description)
        SELECT web, facebook, instagram, email, name, description
        FROM valid
        ON CONFLICT (name) DO UPDATE
          SET web = EXCLUDED.web,
              facebook = EXCLUDED.facebook,
              instagram = EXCLUDED.instagram,
              email = EXCLUDED.email,
              name = EXCLUDED.name,
              description = EXCLUDED.description
        WHERE NOT @Append
        RETURNING xmax = 0 AS inserted
      )
      SELECT
        COUNT(*) FILTER (WHERE inserted)::int AS ""Inserted"",
        COUNT(*) FILTER (WHERE NOT inserted)::int AS ""Updated"",
        (SELECT COUNT(*) FROM input
          WHERE name IS NULL OR description IS NULL)::int
          + CASE WHEN @Append THEN ((SELECT COUNT(*) FROM valid) - (SELECT COUNT(*) FROM upsert))::int ELSE 0 END AS ""Skipped""
      FROM upsert;
    ";

    const string organizationsSql = @"
      WITH input AS (
        SELECT
          row_number() OVER ()::int AS source_row,
          name,
          description,
          email,
          facebook,
          instagram,
          twitter_x,
          web
        FROM jsonb_to_recordset(CAST(@RowsJson AS jsonb)) AS source(
          name text,
          description text,
          email text,
          facebook text,
          instagram text,
          twitter_x text,
          web text)
      ), valid AS (
        SELECT *
        FROM input
        WHERE name IS NOT NULL AND description IS NOT NULL
      ), upsert AS (
        INSERT INTO inelicom.organizations
          (email, facebook, instagram, twitter_x, web, name, description)
        SELECT email, facebook, instagram, twitter_x, web, name, description
        FROM valid
        ON CONFLICT (name) DO UPDATE
          SET email = EXCLUDED.email,
              facebook = EXCLUDED.facebook,
              instagram = EXCLUDED.instagram,
              twitter_x = EXCLUDED.twitter_x,
              web = EXCLUDED.web,
              name = EXCLUDED.name,
              description = EXCLUDED.description
        WHERE NOT @Append
        RETURNING xmax = 0 AS inserted
      )
      SELECT
        COUNT(*) FILTER (WHERE inserted)::int AS ""Inserted"",
        COUNT(*) FILTER (WHERE NOT inserted)::int AS ""Updated"",
        (SELECT COUNT(*) FROM input
          WHERE name IS NULL OR description IS NULL)::int
          + CASE WHEN @Append THEN ((SELECT COUNT(*) FROM valid) - (SELECT COUNT(*) FROM upsert))::int ELSE 0 END AS ""Skipped""
      FROM upsert;
    ";

    try
    {
      var sql = kind switch
      {
        "buildings" => buildingsSql,
        "contacts" => contactsSql,
        "faculties" => facultiesSql,
        "projects" => projectsSql,
        "organizations" => organizationsSql,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported CSV type.")
      };

      var stats = await connection.QuerySingleAsync<CsvImportStats>(
        sql,
        new { RowsJson = rowsJson, Append = append },
        transaction);
      return Result<CsvImportStats, AppError>.Ok(stats);
    }
    catch (Exception ex)
    {
      return Result<CsvImportStats, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }
}
