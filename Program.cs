// Program.cs
using ErgoMind.IoT.Api.Data;
using ErgoMind.IoT.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Pega a connection string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Adiciona o DbContext ao contêiner de serviços, configurando-o para usar Oracle
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseOracle(connectionString)
);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adiciona uma descrição para a v1 no Swagger
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ErgoMind IoT API",
        Version = "v1",
        Description = "API Gateway para receber alertas de IoT (postura e inatividade) [C#]"
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Configura o Swagger UI para usar o endpoint da v1
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ErgoMind IoT API v1");
        options.RoutePrefix = string.Empty; // Acessa o Swagger pela raiz (http://localhost:...)
    });
}

app.UseHttpsRedirection();

// ---- INÍCIO DOS ENDPOINTS DA API ----
// Agrupa todos os endpoints sob o prefixo /api/v1
var apiV1 = app.MapGroup("/api/v1");

apiV1.MapPost("/alertas", async (AlertaIoT novoAlerta, ApiDbContext db) =>
{
    novoAlerta.Timestamp = DateTime.UtcNow;
    db.Alertas.Add(novoAlerta);
    await db.SaveChangesAsync();
    return Results.Created($"/api/v1/alertas/{novoAlerta.Id}", novoAlerta);
})
.WithName("CriarAlerta")
.WithSummary("Registra um novo alerta de IoT (postura ou inatividade).")
.WithOpenApi();

apiV1.MapGet("/alertas", async (ApiDbContext db) =>
{
    var alertas = await db.Alertas.OrderByDescending(a => a.Timestamp).ToListAsync();
    return Results.Ok(alertas);
})
.WithName("ListarAlertas")
.WithSummary("Lista todos os alertas de IoT registrados.")
.WithOpenApi();

apiV1.MapGet("/alertas/{id:int}", async (int id, ApiDbContext db) =>
{
    var alerta = await db.Alertas.FindAsync(id);
    if (alerta == null)
    {
        return Results.NotFound(new { message = "Alerta não encontrado." });
    }
    return Results.Ok(alerta);
})
.WithName("BuscarAlertaPorId")
.WithSummary("Busca um alerta específico pelo seu ID.")
.WithOpenApi();

apiV1.MapPut("/alertas/{id:int}", async (int id, AlertaIoT alertaAtualizado, ApiDbContext db) =>
{
    var alerta = await db.Alertas.FindAsync(id);

    if (alerta == null)
    {
        return Results.NotFound(new { message = "Alerta não encontrado." });
    }

    // Atualiza os campos
    alerta.UsuarioId = alertaAtualizado.UsuarioId;
    alerta.TipoAlerta = alertaAtualizado.TipoAlerta;
    // O Timestamp original é mantido

    db.Alertas.Update(alerta);
    await db.SaveChangesAsync();

    return Results.Ok(alerta);
})
.WithName("AtualizarAlerta")
.WithSummary("Atualiza um alerta de IoT existente.")
.WithOpenApi();

apiV1.MapDelete("/alertas/{id:int}", async (int id, ApiDbContext db) =>
{
    var alerta = await db.Alertas.FindAsync(id);

    if (alerta == null)
    {
        return Results.NotFound(new { message = "Alerta não encontrado." });
    }

    db.Alertas.Remove(alerta);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("ExcluirAlerta")
.WithSummary("Exclui um alerta de IoT específico.")
.WithOpenApi();

// ---- FIM DOS ENDPOINTS DA API ----

app.Run();