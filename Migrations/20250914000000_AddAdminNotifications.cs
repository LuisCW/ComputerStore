using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerStore.Migrations
{
    public partial class AddAdminNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Mensaje = table.Column<string>(maxLength: 500, nullable: false),
                    Tipo = table.Column<string>(maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(nullable: false),
                    Leida = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_UserId",
                table: "AdminNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_Leida",
                table: "AdminNotifications",
                column: "Leida");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_FechaCreacion",
                table: "AdminNotifications",
                column: "FechaCreacion");

            migrationBuilder.CreateTable(
                name: "AdminNotificationReads",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    NotificationId = table.Column<int>(nullable: false),
                    FechaLectura = table.Column<DateTime>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotificationReads_NotificationId",
                table: "AdminNotificationReads",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotificationReads_UserId_NotificationId",
                table: "AdminNotificationReads",
                columns: new[] { "UserId", "NotificationId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotificationReads");

            migrationBuilder.DropTable(
                name: "AdminNotifications");
        }
    }
}
