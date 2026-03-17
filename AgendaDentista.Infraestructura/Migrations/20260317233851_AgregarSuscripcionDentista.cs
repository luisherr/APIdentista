using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaDentista.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSuscripcionDentista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinSuscripcion",
                table: "Dentistas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Dentistas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "Dentistas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SuscripcionActiva",
                table: "Dentistas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaFinSuscripcion",
                table: "Dentistas");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Dentistas");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "Dentistas");

            migrationBuilder.DropColumn(
                name: "SuscripcionActiva",
                table: "Dentistas");
        }
    }
}
