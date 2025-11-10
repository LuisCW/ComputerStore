using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnvioEntityWithNewStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaEnvio",
                table: "Envios",
                newName: "FechaSalidaCentroDistribucion");

            migrationBuilder.AddColumn<string>(
                name: "CentroDistribucionActual",
                table: "Envios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionSecundaria",
                table: "Envios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnRutaEntrega",
                table: "Envios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLlegadaCentroDistribucion",
                table: "Envios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLlegadaCiudadDestino",
                table: "Envios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRecoleccion",
                table: "Envios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotasEntrega",
                table: "Envios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepartidorAsignado",
                table: "Envios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehiculoEntrega",
                table: "Envios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 29, 0, 10, 58, 361, DateTimeKind.Utc).AddTicks(59));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 29, 0, 10, 58, 361, DateTimeKind.Utc).AddTicks(63));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 29, 0, 10, 58, 361, DateTimeKind.Utc).AddTicks(67));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 29, 0, 10, 58, 361, DateTimeKind.Utc).AddTicks(70));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 29, 0, 10, 58, 361, DateTimeKind.Utc).AddTicks(74));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CentroDistribucionActual",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "DireccionSecundaria",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "FechaEnRutaEntrega",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "FechaLlegadaCentroDistribucion",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "FechaLlegadaCiudadDestino",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "FechaRecoleccion",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "NotasEntrega",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "RepartidorAsignado",
                table: "Envios");

            migrationBuilder.DropColumn(
                name: "VehiculoEntrega",
                table: "Envios");

            migrationBuilder.RenameColumn(
                name: "FechaSalidaCentroDistribucion",
                table: "Envios",
                newName: "FechaEnvio");

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 27, 23, 52, 35, 326, DateTimeKind.Utc).AddTicks(2835));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 27, 23, 52, 35, 326, DateTimeKind.Utc).AddTicks(2890));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 27, 23, 52, 35, 326, DateTimeKind.Utc).AddTicks(2893));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 27, 23, 52, 35, 326, DateTimeKind.Utc).AddTicks(2897));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2025, 8, 27, 23, 52, 35, 326, DateTimeKind.Utc).AddTicks(2900));
        }
    }
}
