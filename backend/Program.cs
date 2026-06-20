using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("DATABASE_URL must be set.");

var webappUrl = Environment.GetEnvironmentVariable("WEBAPP_URL") ?? "http://localhost:3000";

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(webappUrl)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton(new Database(connectionString));

var app = builder.Build();

app.UseCors();

app.MapGet("/health", () => Results.Ok(new
{
    service = "backend",
    status = "ok",
    time = DateTimeOffset.UtcNow
}));

app.MapGet("/api/status", async (Database database, CancellationToken cancellationToken) =>
{
    var databaseOnline = await database.CanConnectAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "backend",
        status = databaseOnline ? "ok" : "degraded",
        database = databaseOnline ? "connected" : "unavailable",
        time = DateTimeOffset.UtcNow
    });
});

app.MapGet("/api/catalog/summary", async (Database database, CancellationToken cancellationToken) =>
{
    var summaries = await database.GetTableSummariesAsync(cancellationToken);

    return Results.Ok(new
    {
        schemas = summaries.GroupBy(item => item.Schema)
            .Select(group => new
            {
                name = group.Key,
                tables = group.OrderBy(item => item.Table)
            })
            .OrderBy(item => item.name)
    });
});

app.MapGet("/api/context", async (Database database, CancellationToken cancellationToken) =>
{
    var summaries = await database.GetTableSummariesAsync(cancellationToken);
    var tableNames = summaries
        .Select(item => $"{item.Schema}.{item.Table}")
        .Order()
        .ToArray();

    return Results.Ok(new
    {
        purpose = "University incoming-student Discord support database",
        availableTables = tableNames,
        guidance = "Use this context to answer questions about Discord onboarding, university contacts, places, projects, organizations, roles, permissions, and teams."
    });
});

app.Run();

public sealed record TableSummary(string Schema, string Table, long Rows);

public sealed class Database(string connectionString)
{
    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<TableSummary>> GetTableSummariesAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT table_schema, table_name
            FROM information_schema.tables
            WHERE table_schema IN ('discord', 'inelicom')
              AND table_type = 'BASE TABLE'
            ORDER BY table_schema, table_name;
            """;

        var tables = new List<(string Schema, string Table)>();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var command = new NpgsqlCommand(sql, connection))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add((reader.GetString(0), reader.GetString(1)));
            }
        }

        var result = new List<TableSummary>();

        foreach (var table in tables)
        {
            var identifier = $"{QuoteIdentifier(table.Schema)}.{QuoteIdentifier(table.Table)}";
            await using var countCommand = new NpgsqlCommand($"SELECT COUNT(*) FROM {identifier};", connection);
            var rows = (long)(await countCommand.ExecuteScalarAsync(cancellationToken) ?? 0L);
            result.Add(new TableSummary(table.Schema, table.Table, rows));
        }

        return result;
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
