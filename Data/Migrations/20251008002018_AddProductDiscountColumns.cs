using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDiscountColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinDescuento",
                table: "Productos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioDescuento",
                table: "Productos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoDescuento",
                table: "Productos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeDescuento",
                table: "Productos",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TieneDescuento",
                table: "Productos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "DetailsJson", "FechaFinDescuento", "FechaInicioDescuento", "MotivoDescuento", "PorcentajeDescuento", "TieneDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6253), "{\"\"Procesador\"\":\"\"Intel i7-12700H\"\",\"\"RAM\"\":\"\"16GB DDR4\"\",\"\"Almacenamiento\"\":\"\"512GB NVMe SSD\"\",\"\"Tarjeta Gráfica\"\":\"\"RTX 3060\"\",\"\"Pantalla\"\":\"\"15.6 FHD 144Hz\"\"}", null, null, null, null, false });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "FechaFinDescuento", "FechaInicioDescuento", "MotivoDescuento", "PorcentajeDescuento", "TieneDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6262), null, null, null, null, false });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "DetailsJson", "FechaFinDescuento", "FechaInicioDescuento", "MotivoDescuento", "PorcentajeDescuento", "TieneDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6267), "{\"\"Tamaño\"\":\"\"27 pulgadas\"\",\"\"Resolución\"\":\"\"3840x2160 4K\"\",\"\"Tecnología\"\":\"\"IPS\"\",\"\"Conectividad\"\":\"\"HDMI, DisplayPort, USB-C\"\",\"\"HDR\"\":\"\"HDR10\"\"}", null, null, null, null, false });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "DetailsJson", "FechaFinDescuento", "FechaInicioDescuento", "MotivoDescuento", "PorcentajeDescuento", "TieneDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6320), "{\"\"Procesador\"\":\"\"AMD Ryzen 7 5800X\"\",\"\"RAM\"\":\"\"32GB DDR4\"\",\"\"Almacenamiento\"\":\"\"1TB NVMe SSD\"\",\"\"Tarjeta Gráfica\"\":\"\"NVIDIA RTX 3070\"\",\"\"Fuente\"\":\"\"750W 80+ Gold\"\"}", null, null, null, null, false });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "Description", "DetailsJson", "FechaFinDescuento", "FechaInicioDescuento", "MotivoDescuento", "PorcentajeDescuento", "TieneDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6325), "Procesador de última generación para gaming y productividad", "{\"\"Núcleos\"\":\"\"12\"\",\"\"Hilos\"\":\"\"24\"\",\"\"Frecuencia Base\"\":\"\"4.7 GHz\"\",\"\"Frecuencia Turbo\"\":\"\"5.6 GHz\"\",\"\"Socket\"\":\"\"AM5\"\",\"\"TDP\"\":\"\"170W\"\"}", null, null, null, null, false });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_FechaInicioDescuento_FechaFinDescuento",
                table: "Productos",
                columns: new[] { "FechaInicioDescuento", "FechaFinDescuento" });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_TieneDescuento",
                table: "Productos",
                column: "TieneDescuento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_FechaInicioDescuento_FechaFinDescuento",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_TieneDescuento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaFinDescuento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaInicioDescuento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "MotivoDescuento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PorcentajeDescuento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TieneDescuento",
                table: "Productos");

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4480), "{\"\"Procesador\"\":\"\"Intel i7-12700H\"\",\"\"RAM\"\":\"\"16GB DDR4\"\",\"\"Almacenamiento\"\":\"\"512GB NVMe SSD\"\",\"\"Tarjeta Gr?fica\"\":\"\"RTX 3060\"\",\"\"Pantalla\"\":\"\"15.6 FHD 144Hz\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4492));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4499), "{\"\"Tama?o\"\":\"\"27 pulgadas\"\",\"\"Resoluci?n\"\":\"\"3840x2160 4K\"\",\"\"Tecnolog?a\"\":\"\"IPS\"\",\"\"Conectividad\"\":\"\"HDMI, DisplayPort, USB-C\"\",\"\"HDR\"\":\"\"HDR10\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4505), "{\"\"Procesador\"\":\"\"AMD Ryzen 7 5800X\"\",\"\"RAM\"\":\"\"32GB DDR4\"\",\"\"Almacenamiento\"\":\"\"1TB NVMe SSD\"\",\"\"Tarjeta Gr?fica\"\":\"\"NVIDIA RTX 3070\"\",\"\"Fuente\"\":\"\"750W 80+ Gold\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "Description", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4513), "Procesador de ?ltima generaci?n para gaming y productividad", "{\"\"N?cleos\"\":\"\"12\"\",\"\"Hilos\"\":\"\"24\"\",\"\"Frecuencia Base\"\":\"\"4.7 GHz\"\",\"\"Frecuencia Turbo\"\":\"\"5.6 GHz\"\",\"\"Socket\"\":\"\"AM5\"\",\"\"TDP\"\":\"\"170W\"\"}" });
        }
    }
}
