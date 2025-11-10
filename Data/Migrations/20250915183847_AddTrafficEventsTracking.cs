using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ComputerStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrafficEventsTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Leida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrafficEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Path = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Referrer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Medium = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Campaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Device = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminNotificationReads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificationId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    FechaLectura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotificationReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminNotificationReads_AdminNotifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "AdminNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 5, 18, 38, 46, 737, DateTimeKind.Utc).AddTicks(3179), "{\"\"Procesador\"\":\"\"Intel i7-12700H\"\",\"\"RAM\"\":\"\"16GB DDR4\"\",\"\"Almacenamiento\"\":\"\"512GB NVMe SSD\"\",\"\"Tarjeta Gr?fica\"\":\"\"RTX 3060\"\",\"\"Pantalla\"\":\"\"15.6 FHD 144Hz\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 5, 18, 38, 46, 737, DateTimeKind.Utc).AddTicks(3188), "{\"\"Procesador\"\":\"\"Intel i5-11400\"\",\"\"RAM\"\":\"\"8GB DDR4\"\",\"\"Almacenamiento\"\":\"\"256GB SSD\"\",\"\"Puertos\"\":\"\"USB 3.0, HDMI\"\",\"\"Sistema\"\":\"\"Windows 11 Pro\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 5, 18, 38, 46, 737, DateTimeKind.Utc).AddTicks(3194), "{\"\"Tama?o\"\":\"\"27 pulgadas\"\",\"\"Resoluci?n\"\":\"\"3840x2160 4K\"\",\"\"Tecnolog?a\"\":\"\"IPS\"\",\"\"Conectividad\"\":\"\"HDMI, DisplayPort, USB-C\"\",\"\"HDR\"\":\"\"HDR10\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 5, 18, 38, 46, 737, DateTimeKind.Utc).AddTicks(3199), "{\"\"Procesador\"\":\"\"AMD Ryzen 7 5800X\"\",\"\"RAM\"\":\"\"32GB DDR4\"\",\"\"Almacenamiento\"\":\"\"1TB NVMe SSD\"\",\"\"Tarjeta Gr?fica\"\":\"\"NVIDIA RTX 3070\"\",\"\"Fuente\"\":\"\"750W 80+ Gold\"\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "Description", "DetailsJson" },
                values: new object[] { new DateTime(2025, 9, 5, 18, 38, 46, 737, DateTimeKind.Utc).AddTicks(3203), "Procesador de ?ltima generaci?n para gaming y productividad", "{\"\"N?cleos\"\":\"\"12\"\",\"\"Hilos\"\":\"\"24\"\",\"\"Frecuencia Base\"\":\"\"4.7 GHz\"\",\"\"Frecuencia Turbo\"\":\"\"5.6 GHz\"\",\"\"Socket\"\":\"\"AM5\"\",\"\"TDP\"\":\"\"170W\"\"}" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotificationReads_NotificationId",
                table: "AdminNotificationReads",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotificationReads_UserId_NotificationId",
                table: "AdminNotificationReads",
                columns: new[] { "UserId", "NotificationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_FechaCreacion",
                table: "AdminNotifications",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_Leida",
                table: "AdminNotifications",
                column: "Leida");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_UserId",
                table: "AdminNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_Fecha",
                table: "TrafficEvents",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_Path",
                table: "TrafficEvents",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_UserId",
                table: "TrafficEvents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotificationReads");

            migrationBuilder.DropTable(
                name: "TrafficEvents");

            migrationBuilder.DropTable(
                name: "AdminNotifications");

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 8, 31, 1, 19, 26, 522, DateTimeKind.Utc).AddTicks(2252), "{\"Procesador\":\"Intel i7-12700H\",\"RAM\":\"16GB DDR4\",\"Almacenamiento\":\"512GB NVMe SSD\",\"Tarjeta Gráfica\":\"RTX 3060\",\"Pantalla\":\"15.6 FHD 144Hz\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 8, 31, 1, 19, 26, 522, DateTimeKind.Utc).AddTicks(2264), "{\"Procesador\":\"Intel i5-11400\",\"RAM\":\"8GB DDR4\",\"Almacenamiento\":\"256GB SSD\",\"Puertos\":\"USB 3.0, HDMI\",\"Sistema\":\"Windows 11 Pro\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 8, 31, 1, 19, 26, 522, DateTimeKind.Utc).AddTicks(2272), "{\"Tamaño\":\"27 pulgadas\",\"Resolución\":\"3840x2160 4K\",\"Tecnología\":\"IPS\",\"Conectividad\":\"HDMI, DisplayPort, USB-C\",\"HDR\":\"HDR10\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "DetailsJson" },
                values: new object[] { new DateTime(2025, 8, 31, 1, 19, 26, 522, DateTimeKind.Utc).AddTicks(2336), "{\"Procesador\":\"AMD Ryzen 7 5800X\",\"RAM\":\"32GB DDR4\",\"Almacenamiento\":\"1TB NVMe SSD\",\"Tarjeta Gráfica\":\"NVIDIA RTX 3070\",\"Fuente\":\"750W 80+ Gold\"}" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "Description", "DetailsJson" },
                values: new object[] { new DateTime(2025, 8, 31, 1, 19, 26, 522, DateTimeKind.Utc).AddTicks(2341), "Procesador de última generación para gaming y productividad", "{\"Núcleos\":\"12\",\"Hilos\":\"24\",\"Frecuencia Base\":\"4.7 GHz\",\"Frecuencia Turbo\":\"5.6 GHz\",\"Socket\":\"AM5\",\"TDP\":\"170W\"}" });
        }
    }
}
