// Program.cs
using ErgoMind.IoT.Api.Data;
using ErgoMind.IoT.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Pega a connection string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Adiciona o DbContext ao contêiner de serviços, configurando-o para usar Oracle
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseOracle(connectionString) // Trocado de UseSqlServer para UseOracle
);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adiciona uma descrição para a v1 no Swagger
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ErgoMind IoT API",
        Version = "v1",
        [cite_start]Description = "API Gateway para receber alertas de IoT (postura e inatividade) [cite: 37, 39]"
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

// Agrupa todos os endpoints sob o prefixo /api/v1
[cite_start]
var apiV1 = app.MapGroup("/api/v1");

// Endpoint: POST /api/v1/alertas
[cite_start]// Objetivo: Registrar um novo alerta vindo do sensor IoT
[cite_start]
apiV1.MapPost("/alertas", async (AlertaIoT novoAlerta, ApiDbContext db) =>
{
    // Garante que o timestamp seja gerado pelo servidor
    novoAlerta.Timestamp = DateTime.UtcNow;

    db.Alertas.Add(novoAlerta);
    await db.SaveChangesAsync();

    [cite_start]// Retorna o status 201 Created
    [cite_start]
    return Results.Created($"/api/v1/alertas/{novoAlerta.Id}", novoAlerta);
})
.WithName("CriarAlerta")
.WithSummary("Registra um novo alerta de IoT (postura ou inatividade).")
.WithOpenApi(); // Adiciona ao Swagger

// Endpoint: GET /api/v1/alertas
// Objetivo: Consultar os alertas registrados
[cite_start]// Cumpre: Requisito 1 (Verbo GET)

apiV1.MapGet("/alertas", async (ApiDbContext db) =>
{
    var alertas = await db.Alertas.OrderByDescending(a => a.Timestamp).ToListAsync();
    
    [cite_start]// Retorna status 200 OK
    return Results.Ok(alertas); 
})
.WithName("ListarAlertas")
.WithSummary("Lista todos os alertas de IoT registrados.")
.WithOpenApi(); // Adiciona ao Swagger

// Endpoint: GET /api/v1/alertas/{id}
// Objetivo: Consultar um alerta específico
[cite_start]// Cumpre: Requisito 1 (Verbo GET e Status Codes)

apiV1.MapGet("/alertas/{id:int}", async (int id, ApiDbContext db) =>
{
    var alerta = await db.Alertas.FindAsync(id);

    if (alerta == null)
    {
        [cite_start]// Retorna status 404 Not Found (Boa Prática REST)
        return Results.NotFound(new { message = "Alerta não encontrado." });
    }
    
    [cite_start]// Retorna status 200 OK (Boa Prática REST)
    return Results.Ok(alerta);
})
.WithName("BuscarAlertaPorId")
.WithSummary("Busca um alerta específico pelo seu ID.")
.WithOpenApi(); // Adiciona ao Swagger

app.Run();