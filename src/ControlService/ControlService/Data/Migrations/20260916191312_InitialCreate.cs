using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CargaArchivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreArchivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pendiente"),
                    Periodo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MensajeError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalFilas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargaArchivo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargaArchivo");
        }
    }
}
