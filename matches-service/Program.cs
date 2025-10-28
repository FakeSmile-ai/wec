using Microsoft.EntityFrameworkCore;
using MatchesService.Data;
using MatchesService.Repositories;
using MatchesService.Services;
using MatchesService.Services.Runtime;
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
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// DbContext (SQL Server)
builder.Services.AddDbContext<MatchesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddSingleton<IMatchRunTime, MatchRunTime>();

// SignalR
builder.Services.AddSignalR();

// HttpClient externo
builder.Services.AddHttpClient();

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

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// ==========================================================
// 🔌 ENDPOINTS Y HUBS
// ==========================================================
app.MapControllers();
app.MapHub<ScoreHub>("/hub/score");

// Health (para curl rápido)
app.MapGet("/health", () => Results.Ok("OK"));

// ==========================================================
// 🟢 RUN
// ==========================================================
app.Run();
