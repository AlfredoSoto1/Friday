using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using Dapper;
using Friday.Backend.Api.Repositories;
using Friday.Backend.Api.Services;
using Utils;

var builder = WebApplication.CreateBuilder(args);
var webappUrl = Environment.GetEnvironmentVariable("WEBAPP_URL") ?? "http://localhost:3000";

builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
    policy
      .WithOrigins(webappUrl, "http://localhost:3000", "http://127.0.0.1:3000")
      .AllowAnyHeader()
      .AllowAnyMethod();
  });
});

builder.Services
  .AddControllers()
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.WriteIndented = true;
  });

DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IBotRepository, BotRepository>();
builder.Services.AddScoped<IBotService, BotService>();

var app = builder.Build();

app.UseCors();

app.MapGet("/health", () => Results.Ok(new
{
  service = "backend",
  status = "ok",
  time = DateTimeOffset.UtcNow
}));

app.MapControllers();

app.Run();
