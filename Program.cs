// Program.cs
using ErgoMind.IoT.Api.Data;
using ErgoMind.IoT.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Pega a connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Adiciona o DbContext para Oracle
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseOracle(connectionString)
);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ErgoMind IoT API",
        Version = "v1",
        Description = "API Gateway para receber alertas de IoT (postura e inatividade) [C#]"
    });
});


var app = builder.Build();

// Manda a aplicação aplicar as migrations pendentes ao iniciar.
app.ApplyMigrations();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ErgoMind IoT API v1");
    options.RoutePrefix = string.Empty; // Define o Swagger como página inicial
});

// A linha abaixo foi comentada pois estava causando problemas no Azure
// app.UseHttpsRedirection(); 

// ---- INÍCIO DOS ENDPOINTS DA API ----
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

// ---- CORREÇÃO DA ROTA AQUI ----
apiV1.MapPut("/alertas/{id:int}", async (int id, AlertaIoT alertaAtualizado, ApiDbContext db) =>
{
    var alerta = await db.Alertas.FindAsync(id);
    if (alerta == null)
    {
        return Results.NotFound(new { message = "Alerta não encontrado." });
    }
    alerta.UsuarioId = alertaAtualizado.UsuarioId;
    alerta.TipoAlerta = alertaAtualizado.TipoAlerta;
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


// ---- CLASSE HELPER PARA APLICAR MIGRATIONS ----
public static class DatabaseMigration
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        
        try
        {
            Console.WriteLine("--- Tentando aplicar migrations... ---");
            dbContext.Database.Migrate();
            Console.WriteLine("--- Migrations aplicadas com sucesso. ---");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--- ERRO AO APLICAR MIGRATIONS: {ex.Message} ---");
        }
    }
}