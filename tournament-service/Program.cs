using TournamentService.Services; // <-- La directiva using que faltaba o era incorrecta

var builder = WebApplication.CreateBuilder(args);

var teamsServiceUrl = builder.Configuration["ServiceUrls:TeamsService"];
if (string.IsNullOrEmpty(teamsServiceUrl))
{
    throw new InvalidOperationException("La URL del servicio de equipos no está configurada en appsettings.json.");
}

builder.Services.AddHttpClient<TeamsServiceHttpClient>(client =>
{
    client.BaseAddress = new Uri(teamsServiceUrl);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();