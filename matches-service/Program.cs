using Microsoft.EntityFrameworkCore;
using MatchesService.Data;
using MatchesService.Repositories;
using MatchesService.Services;
using MatchesService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 🔧 CONFIGURACIÓN DE SERVICIOS
// ==========================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
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

// DbContext (SQL Server)
builder.Services.AddDbContext<MatchesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.Configure<TeamsServiceOptions>(builder.Configuration.GetSection("TeamsService"));
builder.Services.AddHttpClient<ITeamClientService, TeamClientService>();

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// ==========================================================
// 🗃️ APLICAR MIGRACIONES EN ARRANQUE
// ==========================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MatchesDbContext>();
    db.Database.Migrate(); // ⬅️ Aplica todas las migraciones pendientes
}

// ==========================================================
// 🌐 MIDDLEWARES
// ==========================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// En docker suele bastar con HTTP
// app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

// ==========================================================
// 🔌 ENDPOINTS Y HUBS
// ==========================================================
app.MapControllers();
app.MapHub<MatchHub>("/hub/matches");

// Health (para curl rápido)
app.MapGet("/health", () => Results.Ok("OK"));

// ==========================================================
// 🟢 RUN
// ==========================================================
app.Run();
