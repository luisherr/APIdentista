using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgendaDentista.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dentistas",
                columns: table => new
                {
                    IdDentista = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SuscripcionActiva = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StripeCustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaFinSuscripcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dentistas", x => x.IdDentista);
                });

            migrationBuilder.CreateTable(
                name: "LogsSistema",
                columns: table => new
                {
                    IdLog = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsSistema", x => x.IdLog);
                });

            migrationBuilder.CreateTable(
                name: "MensajesWhatsApp",
                columns: table => new
                {
                    IdMensaje = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDentista = table.Column<int>(type: "integer", nullable: false),
                    IdPaciente = table.Column<int>(type: "integer", nullable: false),
                    IdCita = table.Column<int>(type: "integer", nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    TipoMensaje = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EstadoEnvio = table.Column<int>(type: "integer", nullable: false),
                    IdMensajeProveedor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesWhatsApp", x => x.IdMensaje);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    IdPaciente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDentista = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.IdPaciente);
                    table.ForeignKey(
                        name: "FK_Pacientes_Dentistas_IdDentista",
                        column: x => x.IdDentista,
                        principalTable: "Dentistas",
                        principalColumn: "IdDentista",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    IdCita = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPaciente = table.Column<int>(type: "integer", nullable: false),
                    IdDentista = table.Column<int>(type: "integer", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tratamiento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    RecordatorioEnviado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.IdCita);
                    table.ForeignKey(
                        name: "FK_Citas_Dentistas_IdDentista",
                        column: x => x.IdDentista,
                        principalTable: "Dentistas",
                        principalColumn: "IdDentista");
                    table.ForeignKey(
                        name: "FK_Citas_Pacientes_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Pacientes",
                        principalColumn: "IdPaciente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversacionesWhatsApp",
                columns: table => new
                {
                    IdConversacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IdPaciente = table.Column<int>(type: "integer", nullable: true),
                    IdDentista = table.Column<int>(type: "integer", nullable: true),
                    HistorialMensajesJson = table.Column<string>(type: "text", nullable: false),
                    UltimaActividad = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
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

            migrationBuilder.CreateTable(
                name: "Recordatorios",
                columns: table => new
                {
                    IdRecordatorio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCita = table.Column<int>(type: "integer", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoRecordatorio = table.Column<int>(type: "integer", nullable: false),
                    EstadoEnvio = table.Column<int>(type: "integer", nullable: false),
                    Intentos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RespuestaPaciente = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recordatorios", x => x.IdRecordatorio);
                    table.ForeignKey(
                        name: "FK_Recordatorios_Citas_IdCita",
                        column: x => x.IdCita,
                        principalTable: "Citas",
                        principalColumn: "IdCita",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_IdDentista_FechaHora",
                table: "Citas",
                columns: new[] { "IdDentista", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_IdPaciente",
                table: "Citas",
                column: "IdPaciente");

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

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_IdMensajeProveedor",
                table: "MensajesWhatsApp",
                column: "IdMensajeProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_IdDentista",
                table: "Pacientes",
                column: "IdDentista");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_Telefono",
                table: "Pacientes",
                column: "Telefono");

            migrationBuilder.CreateIndex(
                name: "IX_Recordatorios_IdCita_TipoRecordatorio",
                table: "Recordatorios",
                columns: new[] { "IdCita", "TipoRecordatorio" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversacionesWhatsApp");

            migrationBuilder.DropTable(
                name: "LogsSistema");

            migrationBuilder.DropTable(
                name: "MensajesWhatsApp");

            migrationBuilder.DropTable(
                name: "Recordatorios");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "Dentistas");
        }
    }
}
