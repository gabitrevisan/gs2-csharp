using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErgoMind.IoT.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialOracleCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UsuarioId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TipoAlerta = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alertas");
        }
    }
}
