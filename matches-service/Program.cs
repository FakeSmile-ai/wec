using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MatchesService.Data;
using MatchesService.Hubs;
using MatchesService.Repositories;
using MatchesService.Services;
using MatchesService.Json;

var builder = WebApplication.CreateBuilder(args);

// Service configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Converters.Add(new TimeOnlyJsonConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]?>() ??
                new[] { "http://localhost", "http://localhost:4200" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddDbContext<MatchesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.Configure<TeamsServiceOptions>(builder.Configuration.GetSection("TeamsService"));
builder.Services.AddHttpClient<ITeamClientService, TeamClientService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Apply migrations with simple retry so the container waits for SQL Server
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<MatchesDbContext>();

    const int maxAttempts = 5;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply migrations (attempt {Attempt}/{MaxAttempts})", attempt, maxAttempts);
            if (attempt == maxAttempts)
            {
                throw;
            }

            var delaySeconds = Math.Pow(2, attempt);
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MatchHub>("/hub/matches");
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();
