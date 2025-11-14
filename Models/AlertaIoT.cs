// Models/AlertaIoT.cs
namespace ErgoMind.IoT.Api.Models
{
    public class AlertaIoT
    {
        // O EF Core usará 'Id' como chave primária por convenção
        public int Id { get; set; }

        [cite_start]// O ID do usuário vindo do sensor ou app
        public string? UsuarioId { get; set; }

        [cite_start]// Tipo de alerta: "Inatividade", "Postura Ruim", etc.
        public string? TipoAlerta { get; set; }

        // Data e hora que o alerta foi recebido
        public DateTime Timestamp { get; set; }
    }
}