using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ComputerStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposDescuentosYCampanasMarketing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoDescuento",
                table: "Productos");

            migrationBuilder.CreateTable(
                name: "MarketingCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreadoPorUserId = table.Column<string>(type: "text", nullable: true),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CodigoPromocional = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProductosAfectados = table.Column<int>(type: "integer", nullable: false),
                    VentasGeneradas = table.Column<int>(type: "integer", nullable: false),
                    MontoVentas = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingCampaigns_AspNetUsers_CreadoPorUserId",
                        column: x => x.CreadoPorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 28, 2, 54, 18, 369, DateTimeKind.Utc).AddTicks(6662), "{\"\"Procesador\"\":\"\"Intel i7-12700H\"\",\"\"RAM\"\":\"\"16GB DDR4\"\",\"\"Almacenamiento\"\":\"\"512GB NVMe SSD\"\",\"\"Tarjeta Gr?fica\"\":\"\"RTX 3060\"\",\"\"Pantalla\"\":\"\"15.6 FHD 144Hz\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 28, 2, 54, 18, 369, DateTimeKind.Utc).AddTicks(6672));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 28, 2, 54, 18, 369, DateTimeKind.Utc).AddTicks(6678), "{\"\"Tama?o\"\":\"\"27 pulgadas\"\",\"\"Resoluci?n\"\":\"\"3840x2160 4K\"\",\"\"Tecnolog?a\"\":\"\"IPS\"\",\"\"Conectividad\"\":\"\"HDMI, DisplayPort, USB-C\"\",\"\"HDR\"\":\"\"HDR10\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 28, 2, 54, 18, 369, DateTimeKind.Utc).AddTicks(6683), "{\"\"Procesador\"\":\"\"AMD Ryzen 7 5800X\"\",\"\"RAM\"\":\"\"32GB DDR4\"\",\"\"Almacenamiento\"\":\"\"1TB NVMe SSD\"\",\"\"Tarjeta Gr?fica\"\":\"\"NVIDIA RTX 3070\"\",\"\"Fuente\"\":\"\"750W 80+ Gold\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "Description", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 28, 2, 54, 18, 369, DateTimeKind.Utc).AddTicks(6688), "Procesador de ?ltima generaci?n para gaming y productividad", "{\"\"N?cleos\"\":\"\"12\"\",\"\"Hilos\"\":\"\"24\"\",\"\"Frecuencia Base\"\":\"\"4.7 GHz\"\",\"\"Frecuencia Turbo\"\":\"\"5.6 GHz\"\",\"\"Socket\"\":\"\"AM5\"\",\"\"TDP\"\":\"\"170W\"\"}" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_Activa",
                table: "MarketingCampaigns",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_Categoria",
                table: "MarketingCampaigns",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_CodigoPromocional",
                table: "MarketingCampaigns",
                column: "CodigoPromocional",
                unique: true,
                filter: "\"CodigoPromocional\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_CreadoPorUserId",
                table: "MarketingCampaigns",
                column: "CreadoPorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_FechaInicio_FechaFin",
                table: "MarketingCampaigns",
                columns: new[] { "FechaInicio", "FechaFin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketingCampaigns");

            migrationBuilder.AddColumn<string>(
                name: "MotivoDescuento",
                table: "Productos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "DetailsJson", "MotivoDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6253), "{\"\"Procesador\"\":\"\"Intel i7-12700H\"\",\"\"RAM\"\":\"\"16GB DDR4\"\",\"\"Almacenamiento\"\":\"\"512GB NVMe SSD\"\",\"\"Tarjeta Gráfica\"\":\"\"RTX 3060\"\",\"\"Pantalla\"\":\"\"15.6 FHD 144Hz\"\"}", null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "MotivoDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6262), null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "DetailsJson", "MotivoDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6267), "{\"\"Tamaño\"\":\"\"27 pulgadas\"\",\"\"Resolución\"\":\"\"3840x2160 4K\"\",\"\"Tecnología\"\":\"\"IPS\"\",\"\"Conectividad\"\":\"\"HDMI, DisplayPort, USB-C\"\",\"\"HDR\"\":\"\"HDR10\"\"}", null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "DetailsJson", "MotivoDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6320), "{\"\"Procesador\"\":\"\"AMD Ryzen 7 5800X\"\",\"\"RAM\"\":\"\"32GB DDR4\"\",\"\"Almacenamiento\"\":\"\"1TB NVMe SSD\"\",\"\"Tarjeta Gráfica\"\":\"\"NVIDIA RTX 3070\"\",\"\"Fuente\"\":\"\"750W 80+ Gold\"\"}", null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "Description", "DetailsJson", "MotivoDescuento" },
                values: new object[] { new DateTime(2025, 9, 28, 0, 20, 17, 58, DateTimeKind.Utc).AddTicks(6325), "Procesador de última generación para gaming y productividad", "{\"\"Núcleos\"\":\"\"12\"\",\"\"Hilos\"\":\"\"24\"\",\"\"Frecuencia Base\"\":\"\"4.7 GHz\"\",\"\"Frecuencia Turbo\"\":\"\"5.6 GHz\"\",\"\"Socket\"\":\"\"AM5\"\",\"\"TDP\"\":\"\"170W\"\"}", null });
        }
    }
}
