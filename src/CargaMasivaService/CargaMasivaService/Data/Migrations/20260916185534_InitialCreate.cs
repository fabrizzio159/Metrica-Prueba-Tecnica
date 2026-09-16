using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargaMasivaService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditoriaFallo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CargaArchivoId = table.Column<int>(type: "int", nullable: false),
                    Fila = table.Column<int>(type: "int", nullable: true),
                    CodigoProducto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DetalleFallo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaFallo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditoriaFallo_CargaArchivo_CargaArchivoId",
                        column: x => x.CargaArchivoId,
                        principalTable: "CargaArchivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataProcesada",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CargaArchivoId = table.Column<int>(type: "int", nullable: false),
                    Periodo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodigoProducto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Procesado"),
                    MensajeError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcesada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcesada_CargaArchivo_CargaArchivoId",
                        column: x => x.CargaArchivoId,
                        principalTable: "CargaArchivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaFallo_CargaArchivoId",
                table: "AuditoriaFallo",
                column: "CargaArchivoId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcesada_CargaArchivoId",
                table: "DataProcesada",
                column: "CargaArchivoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaFallo");

            migrationBuilder.DropTable(
                name: "DataProcesada");
        }
    }
}
