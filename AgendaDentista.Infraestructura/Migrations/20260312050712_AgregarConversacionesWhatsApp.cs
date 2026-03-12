using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaDentista.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConversacionesWhatsApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversacionesWhatsApp",
                columns: table => new
                {
                    IdConversacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdPaciente = table.Column<int>(type: "int", nullable: true),
                    IdDentista = table.Column<int>(type: "int", nullable: true),
                    HistorialMensajesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UltimaActividad = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversacionesWhatsApp", x => x.IdConversacion);
                    table.ForeignKey(
                        name: "FK_ConversacionesWhatsApp_Dentistas_IdDentista",
                        column: x => x.IdDentista,
                        principalTable: "Dentistas",
                        principalColumn: "IdDentista");
                    table.ForeignKey(
                        name: "FK_ConversacionesWhatsApp_Pacientes_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Pacientes",
                        principalColumn: "IdPaciente");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversacionesWhatsApp_IdDentista",
                table: "ConversacionesWhatsApp",
                column: "IdDentista");

            migrationBuilder.CreateIndex(
                name: "IX_ConversacionesWhatsApp_IdPaciente",
                table: "ConversacionesWhatsApp",
                column: "IdPaciente");

            migrationBuilder.CreateIndex(
                name: "IX_ConversacionesWhatsApp_Telefono_Activa",
                table: "ConversacionesWhatsApp",
                columns: new[] { "Telefono", "Activa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversacionesWhatsApp");
        }
    }
}
