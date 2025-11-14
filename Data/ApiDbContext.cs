// Data/ApiDbContext.cs
using ErgoMind.IoT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErgoMind.IoT.Api.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        // Mapeia a classe 'AlertaIoT' para uma tabela 'Alertas' no banco
        public DbSet<AlertaIoT> Alertas { get; set; }
    }
}