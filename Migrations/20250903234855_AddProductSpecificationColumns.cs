using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerStore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSpecificationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Almacenamiento",
                table: "Productos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Botones",
                table: "Productos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conectividad",
                table: "Productos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DPI",
                table: "Productos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frecuencia",
                table: "Productos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Garantia",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HDR",
                table: "Productos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Layout",
                table: "Productos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Panel",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pantalla",
                table: "Productos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Peso",
                table: "Productos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Procesador",
                table: "Productos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RAM",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resolucion",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Retroiluminacion",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SistemaOperativo",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Switch",
                table: "Productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TarjetaGrafica",
                table: "Productos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Almacenamiento", "Botones", "Conectividad", "CreatedDate", "DPI", "Frecuencia", "Garantia", "HDR", "Layout", "Panel", "Pantalla", "Peso", "Procesador", "RAM", "Resolucion", "Retroiluminacion", "SistemaOperativo", "Switch", "TarjetaGrafica" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 24, 23, 48, 54, 711, DateTimeKind.Utc).AddTicks(8926), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Almacenamiento", "Botones", "Conectividad", "CreatedDate", "DPI", "Frecuencia", "Garantia", "HDR", "Layout", "Panel", "Pantalla", "Peso", "Procesador", "RAM", "Resolucion", "Retroiluminacion", "SistemaOperativo", "Switch", "TarjetaGrafica" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 24, 23, 48, 54, 711, DateTimeKind.Utc).AddTicks(8936), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Almacenamiento", "Botones", "Conectividad", "CreatedDate", "DPI", "Frecuencia", "Garantia", "HDR", "Layout", "Panel", "Pantalla", "Peso", "Procesador", "RAM", "Resolucion", "Retroiluminacion", "SistemaOperativo", "Switch", "TarjetaGrafica" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 24, 23, 48, 54, 711, DateTimeKind.Utc).AddTicks(8940), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Almacenamiento", "Botones", "Conectividad", "CreatedDate", "DPI", "Frecuencia", "Garantia", "HDR", "Layout", "Panel", "Pantalla", "Peso", "Procesador", "RAM", "Resolucion", "Retroiluminacion", "SistemaOperativo", "Switch", "TarjetaGrafica" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 24, 23, 48, 54, 711, DateTimeKind.Utc).AddTicks(8945), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Almacenamiento", "Botones", "Conectividad", "CreatedDate", "DPI", "Frecuencia", "Garantia", "HDR", "Layout", "Panel", "Pantalla", "Peso", "Procesador", "RAM", "Resolucion", "Retroiluminacion", "SistemaOperativo", "Switch", "TarjetaGrafica" },
                values: new object[] { null, null, null, new DateTime(2025, 8, 24, 23, 48, 54, 711, DateTimeKind.Utc).AddTicks(8950), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Almacenamiento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Botones",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Conectividad",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DPI",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Frecuencia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Garantia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "HDR",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Layout",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Panel",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Pantalla",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Peso",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Procesador",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "RAM",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Resolucion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Retroiluminacion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "SistemaOperativo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Switch",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TarjetaGrafica",
                table: "Productos");

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6100));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6107));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6112));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6117));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6122));
        }
    }
}
