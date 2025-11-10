using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ComputerStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketingCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrafficEvents_Pedidos_PedidoId",
                table: "TrafficEvents");

            migrationBuilder.DropTable(
                name: "MarketingChannelMetrics");

            migrationBuilder.DropTable(
                name: "PedidoMarketingSources");

            migrationBuilder.DropIndex(
                name: "IX_TrafficEvents_Canal",
                table: "TrafficEvents");

            migrationBuilder.DropIndex(
                name: "IX_TrafficEvents_IsConversion",
                table: "TrafficEvents");

            migrationBuilder.DropIndex(
                name: "IX_TrafficEvents_SessionId",
                table: "TrafficEvents");

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 142);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 143);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "MarketingCosts",
                keyColumn: "Id",
                keyValue: 147);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "TrafficEvents",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "TipoGasto",
                table: "MarketingCosts");

            migrationBuilder.RenameColumn(
                name: "Costo",
                table: "MarketingCosts",
                newName: "CostoTotal");

            migrationBuilder.RenameColumn(
                name: "CampaignId",
                table: "MarketingCosts",
                newName: "PlataformaPublicidad");

            migrationBuilder.AlterColumn<decimal>(
                name: "ConversionValue",
                table: "TrafficEvents",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Canal",
                table: "MarketingCosts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Campana",
                table: "MarketingCosts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CampanaIdExterna",
                table: "MarketingCosts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Clicks",
                table: "MarketingCosts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoPorClick",
                table: "MarketingCosts",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Impresiones",
                table: "MarketingCosts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "MarketingCosts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoCampana",
                table: "MarketingCosts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 59, 36, 184, DateTimeKind.Utc).AddTicks(4886));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 59, 36, 184, DateTimeKind.Utc).AddTicks(4896));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 59, 36, 184, DateTimeKind.Utc).AddTicks(4900));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 59, 36, 184, DateTimeKind.Utc).AddTicks(4905));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 59, 36, 184, DateTimeKind.Utc).AddTicks(4910));

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_Source_Medium_Campaign",
                table: "TrafficEvents",
                columns: new[] { "Source", "Medium", "Campaign" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCosts_Campana",
                table: "MarketingCosts",
                column: "Campana");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCosts_Canal_Fecha",
                table: "MarketingCosts",
                columns: new[] { "Canal", "Fecha" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrafficEvents_Pedidos_PedidoId",
                table: "TrafficEvents",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrafficEvents_Pedidos_PedidoId",
                table: "TrafficEvents");

            migrationBuilder.DropIndex(
                name: "IX_TrafficEvents_Source_Medium_Campaign",
                table: "TrafficEvents");

            migrationBuilder.DropIndex(
                name: "IX_MarketingCosts_Campana",
                table: "MarketingCosts");

            migrationBuilder.DropIndex(
                name: "IX_MarketingCosts_Canal_Fecha",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "Campana",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "CampanaIdExterna",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "Clicks",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "CostoPorClick",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "Impresiones",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "TipoCampana",
                table: "MarketingCosts");

            migrationBuilder.RenameColumn(
                name: "PlataformaPublicidad",
                table: "MarketingCosts",
                newName: "CampaignId");

            migrationBuilder.RenameColumn(
                name: "CostoTotal",
                table: "MarketingCosts",
                newName: "Costo");

            migrationBuilder.AlterColumn<decimal>(
                name: "ConversionValue",
                table: "TrafficEvents",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Canal",
                table: "MarketingCosts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "MarketingCosts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "MarketingCosts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TipoGasto",
                table: "MarketingCosts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MarketingChannelMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Canal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Clics = table.Column<int>(type: "integer", nullable: false),
                    Conversiones = table.Column<int>(type: "integer", nullable: false),
                    CostoTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Impresiones = table.Column<int>(type: "integer", nullable: false),
                    IngresoGenerado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notas = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingChannelMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PedidoMarketingSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    Campaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Canal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Direct"),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Medium = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Referrer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UtmCampaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UtmContent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UtmMedium = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UtmSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UtmTerm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoMarketingSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoMarketingSources_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MarketingChannelMetrics",
                columns: new[] { "Id", "Canal", "Clics", "Conversiones", "CostoTotal", "Fecha", "FechaActualizacion", "Impresiones", "IngresoGenerado", "Notas" },
                values: new object[,]
                {
                    { 1, "Google Ads", 374, 10, 79908m, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), 11809, 1447843m, null },
                    { 2, "Facebook Ads", 471, 4, 67214m, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), 17265, 998963m, null },
                    { 3, "SEO", 212, 12, 50000m, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), 4675, 1989511m, null },
                    { 4, "Email Marketing", 80, 7, 15000m, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), 1589, 1357208m, null },
                    { 5, "Social Media", 141, 5, 29201m, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), 9263, 1015951m, null },
                    { 6, "Google Ads", 567, 14, 102613m, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), 15065, 2776363m, null },
                    { 7, "Facebook Ads", 412, 11, 94524m, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), 28790, 2183969m, null },
                    { 8, "SEO", 409, 16, 0m, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), 8347, 2911298m, null },
                    { 9, "Email Marketing", 186, 10, 0m, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), 2329, 1601205m, null },
                    { 10, "Social Media", 203, 7, 34135m, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), 15200, 1087468m, null },
                    { 11, "Google Ads", 581, 13, 127318m, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), 12321, 2984883m, null },
                    { 12, "Facebook Ads", 453, 8, 89835m, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), 30314, 1848975m, null },
                    { 13, "SEO", 329, 14, 0m, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), 8818, 3553085m, null },
                    { 14, "Email Marketing", 219, 10, 0m, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), 2269, 1805202m, null },
                    { 15, "Social Media", 288, 7, 27069m, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), 10336, 1178986m, null },
                    { 16, "Google Ads", 594, 13, 102023m, new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), 17577, 3193403m, null },
                    { 17, "Facebook Ads", 494, 11, 85146m, new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), 31839, 1513982m, null },
                    { 18, "SEO", 399, 12, 0m, new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), 9289, 3194872m, null },
                    { 19, "Email Marketing", 163, 10, 15000m, new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), 2208, 1409199m, null },
                    { 20, "Social Media", 253, 7, 20003m, new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), 11473, 1270503m, null },
                    { 21, "Google Ads", 358, 12, 126728m, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), 14833, 3401924m, null },
                    { 22, "Facebook Ads", 535, 8, 80456m, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), 20364, 1978988m, null },
                    { 23, "SEO", 319, 21, 0m, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), 9761, 2836659m, null },
                    { 24, "Email Marketing", 196, 9, 0m, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), 2148, 1613196m, null },
                    { 25, "Social Media", 218, 6, 37936m, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), 12610, 1362021m, null },
                    { 26, "Google Ads", 371, 11, 101433m, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), 12089, 2310444m, null },
                    { 27, "Facebook Ads", 576, 6, 75767m, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), 21889, 1643994m, null },
                    { 28, "SEO", 388, 19, 0m, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), 7232, 3478446m, null },
                    { 29, "Email Marketing", 229, 9, 0m, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), 2088, 1817194m, null },
                    { 30, "Social Media", 304, 6, 30870m, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), 13747, 1453538m, null },
                    { 31, "Google Ads", 205, 5, 78138m, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), 11344, 1398964m, null },
                    { 32, "Facebook Ads", 417, 5, 39077m, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), 13413, 1389000m, null },
                    { 33, "SEO", 180, 11, 0m, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), 4503, 1840233m, null },
                    { 34, "Email Marketing", 101, 5, 15000m, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), 1228, 781191m, null },
                    { 35, "Social Media", 173, 2, 11804m, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), 10084, 565056m, null },
                    { 36, "Google Ads", 218, 4, 52843m, new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), 8600, 1607484m, null },
                    { 37, "Facebook Ads", 458, 2, 34388m, new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), 14938, 1054006m, null },
                    { 38, "SEO", 250, 9, 50000m, new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), 4975, 2482020m, null },
                    { 39, "Email Marketing", 134, 4, 0m, new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), 1168, 985188m, null },
                    { 40, "Social Media", 138, 2, 29738m, new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), 5221, 656573m, null },
                    { 41, "Google Ads", 412, 17, 125548m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19856, 2936004m, null },
                    { 42, "Facebook Ads", 699, 9, 96698m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26463, 2239012m, null },
                    { 43, "SEO", 298, 13, 0m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8646, 3403806m, null },
                    { 44, "Email Marketing", 150, 8, 0m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1908, 1829185m, null },
                    { 45, "Social Media", 319, 5, 34672m, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11158, 1228091m, null },
                    { 46, "Google Ads", 425, 16, 100253m, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), 17112, 3144525m, null },
                    { 47, "Facebook Ads", 440, 6, 92009m, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), 27987, 1904018m, null },
                    { 48, "SEO", 367, 22, 0m, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), 9117, 3045593m, null },
                    { 49, "Email Marketing", 183, 8, 15000m, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1848, 1433182m, null },
                    { 50, "Social Media", 285, 4, 27606m, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), 12295, 1319608m, null },
                    { 51, "Google Ads", 439, 15, 124958m, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), 14368, 3353045m, null },
                    { 52, "Facebook Ads", 480, 9, 87320m, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), 29512, 1569025m, null },
                    { 53, "SEO", 287, 20, 0m, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), 9589, 3687380m, null },
                    { 54, "Email Marketing", 217, 7, 0m, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), 2388, 1637179m, null },
                    { 55, "Social Media", 250, 4, 20540m, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), 13432, 1411126m, null },
                    { 56, "Google Ads", 452, 14, 149663m, new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), 19623, 3561565m, null },
                    { 57, "Facebook Ads", 521, 6, 82630m, new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), 31037, 2034031m, null },
                    { 58, "SEO", 357, 18, 0m, new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), 7060, 3329167m, null },
                    { 59, "Email Marketing", 160, 7, 0m, new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), 2327, 1841177m, null },
                    { 60, "Social Media", 215, 4, 38474m, new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), 14569, 1002643m, null },
                    { 61, "Google Ads", 466, 13, 124368m, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), 16879, 2470085m, null },
                    { 62, "Facebook Ads", 562, 9, 77941m, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), 32561, 1699037m, null },
                    { 63, "SEO", 277, 16, 0m, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), 7531, 2970954m, null },
                    { 64, "Email Marketing", 194, 7, 15000m, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), 2267, 1445174m, null },
                    { 65, "Social Media", 300, 8, 31408m, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), 15706, 1094161m, null },
                    { 66, "Google Ads", 299, 7, 101073m, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), 8135, 1558606m, null },
                    { 67, "Facebook Ads", 403, 2, 41251m, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), 11086, 1444043m, null },
                    { 68, "SEO", 218, 8, 0m, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), 4803, 2332741m, null },
                    { 69, "Email Marketing", 155, 2, 0m, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), 1407, 1009171m, null },
                    { 70, "Social Media", 170, 5, 12342m, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), 6043, 705679m, null },
                    { 71, "Google Ads", 313, 6, 75778m, new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), 13391, 1767126m, null },
                    { 72, "Facebook Ads", 444, 5, 36562m, new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), 12611, 1109049m, null },
                    { 73, "SEO", 288, 6, 50000m, new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), 5274, 1974528m, null },
                    { 74, "Email Marketing", 99, 2, 0m, new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), 1347, 1213168m, null },
                    { 75, "Social Media", 135, 5, 30276m, new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), 7180, 797196m, null },
                    { 76, "Google Ads", 506, 10, 148483m, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), 16647, 3095646m, null },
                    { 77, "Facebook Ads", 685, 6, 98873m, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), 24136, 2294055m, null },
                    { 78, "SEO", 336, 20, 0m, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), 8945, 2896315m, null },
                    { 79, "Email Marketing", 204, 6, 15000m, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2087, 1457165m, null },
                    { 80, "Social Media", 316, 7, 35210m, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), 13117, 1368714m, null },
                    { 81, "Google Ads", 520, 9, 123188m, new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), 13903, 3304166m, null },
                    { 82, "Facebook Ads", 426, 9, 94183m, new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), 25660, 1959061m, null },
                    { 83, "SEO", 406, 18, 0m, new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), 9417, 3538102m, null },
                    { 84, "Email Marketing", 237, 11, 0m, new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), 2027, 1661163m, null },
                    { 85, "Social Media", 281, 7, 28144m, new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), 14254, 1460231m, null },
                    { 86, "Google Ads", 533, 17, 147893m, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), 19158, 3512687m, null },
                    { 87, "Facebook Ads", 467, 6, 89494m, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), 27185, 1624068m, null },
                    { 88, "SEO", 325, 16, 0m, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), 9888, 3179888m, null },
                    { 89, "Email Marketing", 181, 11, 0m, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), 1967, 1865160m, null },
                    { 90, "Social Media", 247, 7, 21077m, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), 15391, 1051749m, null },
                    { 91, "Google Ads", 547, 16, 122598m, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), 16414, 2421207m, null },
                    { 92, "Facebook Ads", 508, 9, 84804m, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), 28710, 2089074m, null },
                    { 93, "SEO", 395, 14, 0m, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), 7359, 2821675m, null },
                    { 94, "Email Marketing", 214, 10, 15000m, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1907, 1469157m, null },
                    { 95, "Social Media", 212, 6, 39011m, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), 10528, 1143266m, null },
                    { 96, "Google Ads", 560, 15, 147303m, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), 13670, 2629727m, null },
                    { 97, "Facebook Ads", 549, 6, 80115m, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), 30234, 1754080m, null },
                    { 98, "SEO", 315, 12, 0m, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), 7831, 3463462m, null },
                    { 99, "Email Marketing", 158, 10, 0m, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), 1847, 1673154m, null },
                    { 100, "Social Media", 297, 6, 31945m, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), 11665, 1234784m, null },
                    { 101, "Google Ads", 393, 9, 74008m, new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), 12926, 1718247m, null },
                    { 102, "Facebook Ads", 390, 6, 43426m, new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), 21759, 1499086m, null },
                    { 103, "SEO", 257, 15, 0m, new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), 5102, 1825249m, null },
                    { 104, "Email Marketing", 119, 6, 0m, new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), 1586, 1237151m, null },
                    { 105, "Social Media", 166, 3, 12879m, new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), 8002, 846301m, null },
                    { 106, "Google Ads", 407, 8, 98713m, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), 10182, 1926767m, null },
                    { 107, "Facebook Ads", 431, 3, 38736m, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), 10284, 1164092m, null },
                    { 108, "SEO", 176, 13, 50000m, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), 5573, 2467036m, null },
                    { 109, "Email Marketing", 153, 5, 15000m, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), 1526, 841148m, null },
                    { 110, "Social Media", 132, 2, 30813m, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), 9139, 937819m, null },
                    { 111, "Google Ads", 350, 12, 121418m, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), 13437, 3255288m, null },
                    { 112, "Facebook Ads", 672, 10, 66047m, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), 21808, 1549098m, null },
                    { 113, "SEO", 374, 17, 0m, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), 9245, 3388823m, null },
                    { 114, "Email Marketing", 168, 9, 0m, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2266, 1685146m, null },
                    { 115, "Social Media", 313, 5, 35747m, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), 15076, 1009336m, null },
                    { 116, "Google Ads", 364, 11, 146123m, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), 18693, 3463808m, null },
                    { 117, "Facebook Ads", 412, 7, 96357m, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), 23333, 2014104m, null },
                    { 118, "SEO", 294, 15, 0m, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), 9716, 3030610m, null },
                    { 119, "Email Marketing", 202, 9, 0m, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), 2206, 1889143m, null },
                    { 120, "Social Media", 278, 5, 28681m, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), 10213, 1100854m, null },
                    { 121, "Google Ads", 377, 10, 120828m, new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), 15949, 2372328m, null },
                    { 122, "Facebook Ads", 453, 10, 91668m, new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), 24858, 1679111m, null },
                    { 123, "SEO", 364, 13, 0m, new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), 7187, 3672397m, null },
                    { 124, "Email Marketing", 235, 8, 15000m, new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), 2146, 1493140m, null },
                    { 125, "Social Media", 243, 4, 21615m, new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), 11350, 1192371m, null },
                    { 126, "Google Ads", 391, 9, 145533m, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), 13205, 2580848m, null },
                    { 127, "Facebook Ads", 494, 7, 86979m, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), 26383, 2144117m, null },
                    { 128, "SEO", 283, 22, 0m, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), 7659, 3314183m, null },
                    { 129, "Email Marketing", 178, 8, 0m, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2086, 1697137m, null },
                    { 130, "Social Media", 209, 4, 39549m, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), 12486, 1283889m, null },
                    { 131, "Google Ads", 404, 17, 120238m, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), 18461, 2789369m, null },
                    { 132, "Facebook Ads", 535, 10, 82289m, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), 27907, 1809123m, null },
                    { 133, "SEO", 353, 20, 0m, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), 8130, 2955970m, null },
                    { 134, "Email Marketing", 212, 8, 0m, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), 2026, 1901134m, null },
                    { 135, "Social Media", 294, 8, 32483m, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), 13623, 1375407m, null },
                    { 136, "Google Ads", 238, 11, 96943m, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), 9717, 1877889m, null },
                    { 137, "Facebook Ads", 376, 3, 45600m, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), 19432, 1554129m, null },
                    { 138, "SEO", 145, 12, 0m, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), 5401, 2317757m, null },
                    { 139, "Email Marketing", 83, 3, 15000m, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), 1166, 865131m, null },
                    { 140, "Social Media", 163, 5, 13417m, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), 9960, 986924m, null },
                    { 141, "Google Ads", 251, 10, 71648m, new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), 6972, 2086409m, null },
                    { 142, "Facebook Ads", 417, 6, 40910m, new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), 20957, 1219135m, null },
                    { 143, "SEO", 215, 10, 50000m, new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), 5873, 1959544m, null },
                    { 144, "Email Marketing", 117, 3, 0m, new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), 1105, 1069129m, null },
                    { 145, "Social Media", 128, 5, 31351m, new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), 11097, 578442m, null },
                    { 146, "Google Ads", 445, 14, 144353m, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), 18228, 3414929m, null },
                    { 147, "Facebook Ads", 658, 7, 68221m, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), 32481, 1604141m, null },
                    { 148, "SEO", 412, 14, 0m, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), 9544, 2881331m, null },
                    { 149, "Email Marketing", 222, 7, 0m, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), 1845, 1913126m, null },
                    { 150, "Social Media", 310, 7, 36285m, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), 11034, 1149959m, null }
                });

            migrationBuilder.InsertData(
                table: "MarketingCosts",
                columns: new[] { "Id", "CampaignId", "Canal", "Costo", "Descripcion", "Fecha", "FechaCreacion", "TipoGasto" },
                values: new object[,]
                {
                    { 1, null, "Google Ads", 88312m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 2, null, "Facebook Ads", 41703m, "Campañas Facebook e Instagram", new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 3, null, "SEO", 50000m, "Herramientas SEO + creación contenido", new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "SEO" },
                    { 4, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 6, null, "Google Ads", 112433m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 7, null, "Facebook Ads", 91988m, "Campañas Facebook e Instagram", new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 11, null, "Google Ads", 138554m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 12, null, "Facebook Ads", 75273m, "Campañas Facebook e Instagram", new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 16, null, "Google Ads", 114675m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 17, null, "Facebook Ads", 93558m, "Campañas Facebook e Instagram", new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 19, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 21, null, "Google Ads", 140797m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 22, null, "Facebook Ads", 76842m, "Campañas Facebook e Instagram", new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 26, null, "Google Ads", 116918m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 27, null, "Facebook Ads", 95127m, "Campañas Facebook e Instagram", new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 31, null, "Google Ads", 95039m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 32, null, "Facebook Ads", 46412m, "Campañas Facebook e Instagram", new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 34, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 36, null, "Google Ads", 71161m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 37, null, "Facebook Ads", 64697m, "Campañas Facebook e Instagram", new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 38, null, "SEO", 50000m, "Herramientas SEO + creación contenido", new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "SEO" },
                    { 41, null, "Google Ads", 145282m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 42, null, "Facebook Ads", 79982m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 46, null, "Google Ads", 121403m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 47, null, "Facebook Ads", 98267m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 49, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 51, null, "Google Ads", 147524m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 52, null, "Facebook Ads", 81552m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 56, null, "Google Ads", 123646m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 57, null, "Facebook Ads", 99837m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 61, null, "Google Ads", 149767m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 62, null, "Facebook Ads", 83122m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 64, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 66, null, "Google Ads", 77888m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 67, null, "Facebook Ads", 34406m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 71, null, "Google Ads", 54009m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 72, null, "Facebook Ads", 52691m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 73, null, "SEO", 50000m, "Herramientas SEO + creación contenido", new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "SEO" },
                    { 76, null, "Google Ads", 128131m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 77, null, "Facebook Ads", 67976m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 79, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 81, null, "Google Ads", 104252m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 82, null, "Facebook Ads", 86261m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 86, null, "Google Ads", 130373m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 87, null, "Facebook Ads", 69546m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 91, null, "Google Ads", 106494m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 92, null, "Facebook Ads", 87831m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 94, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 96, null, "Google Ads", 132616m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 97, null, "Facebook Ads", 71116m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 101, null, "Google Ads", 60737m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 102, null, "Facebook Ads", 57401m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 106, null, "Google Ads", 86858m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 107, null, "Facebook Ads", 40686m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 108, null, "SEO", 50000m, "Herramientas SEO + creación contenido", new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), "SEO" },
                    { 109, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 111, null, "Google Ads", 110980m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 112, null, "Facebook Ads", 90970m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 116, null, "Google Ads", 137101m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 117, null, "Facebook Ads", 74255m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 121, null, "Google Ads", 113222m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 122, null, "Facebook Ads", 92540m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 124, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 126, null, "Google Ads", 139343m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 127, null, "Facebook Ads", 75825m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 131, null, "Google Ads", 115465m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 132, null, "Facebook Ads", 94110m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 136, null, "Google Ads", 93586m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 137, null, "Facebook Ads", 45395m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 139, null, "Email Marketing", 15000m, "Mailchimp + diseño newsletters", new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { 141, null, "Google Ads", 69707m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 142, null, "Facebook Ads", 63680m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 143, null, "SEO", 50000m, "Herramientas SEO + creación contenido", new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), "SEO" },
                    { 146, null, "Google Ads", 143828m, "Campaña búsquedas Google - laptops gaming", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" },
                    { 147, null, "Facebook Ads", 78965m, "Campañas Facebook e Instagram", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Publicidad" }
                });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 34, 37, 682, DateTimeKind.Utc).AddTicks(7519));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 34, 37, 682, DateTimeKind.Utc).AddTicks(7531));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 34, 37, 682, DateTimeKind.Utc).AddTicks(7536));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 34, 37, 682, DateTimeKind.Utc).AddTicks(7542));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 12, 21, 34, 37, 682, DateTimeKind.Utc).AddTicks(7547));

            migrationBuilder.InsertData(
                table: "TrafficEvents",
                columns: new[] { "Id", "Campaign", "Canal", "City", "ConversionValue", "Country", "Device", "Fecha", "Ip", "IsBounce", "IsConversion", "LandingPage", "Medium", "PageViews", "Path", "PedidoId", "Referrer", "SessionId", "Source", "TimeOnSite", "UserAgent", "UserId", "UtmCampaign", "UtmContent", "UtmMedium", "UtmSource", "UtmTerm" },
                values: new object[,]
                {
                    { 1, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Products/Gaming", null, null, "session_0_20250901", "direct", 443, null, null, null, null, null, null, null },
                    { 2, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/", null, null, "session_1_20250915", "googleads", 171, null, null, null, null, null, null, null },
                    { 3, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_2_20250830", "direct", 469, null, null, null, null, null, null, null },
                    { 4, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_3_20250914", "googleads", 197, null, null, null, null, null, null, null },
                    { 5, null, "Direct", "Cartagena", null, "CO", null, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_4_20250829", "direct", 495, null, null, null, null, null, null, null },
                    { 6, null, "Facebook Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_5_20250912", "facebookads", 222, null, null, null, null, null, null, null },
                    { 7, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_6_20250828", "socialmedia", 520, null, null, null, null, null, null, null },
                    { 8, null, "Facebook Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_7_20250911", "facebookads", 248, null, null, null, null, null, null, null },
                    { 9, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_8_20250826", "socialmedia", 546, null, null, null, null, null, null, null },
                    { 10, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_9_20250910", "facebookads", 274, null, null, null, null, null, null, null },
                    { 11, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_10_20250825", "socialmedia", 571, null, null, null, null, null, null, null },
                    { 12, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products", null, null, "session_11_20250908", "facebookads", 299, null, null, null, null, null, null, null },
                    { 13, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_12_20250824", "socialmedia", 597, null, null, null, null, null, null, null },
                    { 14, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products/Gaming", null, null, "session_13_20250907", "emailmarketing", 325, null, null, null, null, null, null, null },
                    { 15, null, "SEO", "Bogotá", 948731m, "CO", null, new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, null, "organic", 1, "/", null, null, "session_14_20250921", "seo", 52, null, null, null, null, null, null, null },
                    { 16, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products/Gaming", null, null, "session_15_20250906", "emailmarketing", 350, null, null, null, null, null, null, null },
                    { 17, null, "SEO", "Bogotá", null, "CO", null, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "organic", 1, "/", null, null, "session_16_20250920", "seo", 78, null, null, null, null, null, null, null },
                    { 18, null, "Email Marketing", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 5, "/Products/Gaming", null, null, "session_17_20250904", "emailmarketing", 376, null, null, null, null, null, null, null },
                    { 19, null, "SEO", "Bogotá", null, "CO", null, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "organic", 1, "/", null, null, "session_18_20250919", "seo", 104, null, null, null, null, null, null, null },
                    { 20, null, "Email Marketing", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 5, "/Products/Gaming", null, null, "session_19_20250903", "emailmarketing", 401, null, null, null, null, null, null, null },
                    { 21, null, "Google Ads", "Bogotá", null, "CO", null, new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/", null, null, "session_20_20250917", "googleads", 129, null, null, null, null, null, null, null },
                    { 22, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 5, "/Products/Gaming", null, null, "session_21_20250902", "direct", 427, null, null, null, null, null, null, null },
                    { 23, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/", null, null, "session_22_20250916", "googleads", 155, null, null, null, null, null, null, null },
                    { 24, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Products/Gaming", null, null, "session_23_20250831", "direct", 452, null, null, null, null, null, null, null },
                    { 25, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/Products", null, null, "session_24_20250915", "googleads", 180, null, null, null, null, null, null, null },
                    { 26, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_25_20250830", "direct", 478, null, null, null, null, null, null, null },
                    { 27, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_26_20250913", "googleads", 206, null, null, null, null, null, null, null },
                    { 28, null, "Direct", "Cartagena", null, "CO", null, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_27_20250829", "direct", 504, null, null, null, null, null, null, null },
                    { 29, null, "Facebook Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_28_20250912", "facebookads", 231, null, null, null, null, null, null, null },
                    { 30, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_29_20250827", "socialmedia", 529, null, null, null, null, null, null, null },
                    { 31, null, "Facebook Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_30_20250911", "facebookads", 257, null, null, null, null, null, null, null },
                    { 32, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_31_20250826", "socialmedia", 555, null, null, null, null, null, null, null },
                    { 33, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 4, "/Products", null, null, "session_32_20250909", "facebookads", 282, null, null, null, null, null, null, null },
                    { 34, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_33_20250825", "socialmedia", 580, null, null, null, null, null, null, null },
                    { 35, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products", null, null, "session_34_20250908", "facebookads", 308, null, null, null, null, null, null, null },
                    { 36, null, "SEO", "Bogotá", 841178m, "CO", null, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, null, "organic", 1, "/", null, null, "session_35_20250922", "seo", 36, null, null, null, null, null, null, null },
                    { 37, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products/Gaming", null, null, "session_36_20250906", "emailmarketing", 334, null, null, null, null, null, null, null },
                    { 38, null, "SEO", "Bogotá", 1007125m, "CO", null, new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, null, "organic", 1, "/", null, null, "session_37_20250921", "seo", 61, null, null, null, null, null, null, null },
                    { 39, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 5, "/Products/Gaming", null, null, "session_38_20250905", "emailmarketing", 359, null, null, null, null, null, null, null },
                    { 40, null, "SEO", "Bogotá", null, "CO", null, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "organic", 1, "/", null, null, "session_39_20250919", "seo", 87, null, null, null, null, null, null, null },
                    { 41, null, "Email Marketing", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 5, "/Products/Gaming", null, null, "session_40_20250904", "emailmarketing", 385, null, null, null, null, null, null, null },
                    { 42, null, "SEO", "Bogotá", null, "CO", null, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "organic", 2, "/", null, null, "session_41_20250918", "seo", 113, null, null, null, null, null, null, null },
                    { 43, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 5, "/Products/Gaming", null, null, "session_42_20250902", "direct", 410, null, null, null, null, null, null, null },
                    { 44, null, "Google Ads", "Bogotá", null, "CO", null, new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/", null, null, "session_43_20250917", "googleads", 138, null, null, null, null, null, null, null },
                    { 45, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 5, "/Products/Gaming", null, null, "session_44_20250901", "direct", 436, null, null, null, null, null, null, null },
                    { 46, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/", null, null, "session_45_20250915", "googleads", 164, null, null, null, null, null, null, null },
                    { 47, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_46_20250831", "direct", 461, null, null, null, null, null, null, null },
                    { 48, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/Products", null, null, "session_47_20250914", "googleads", 189, null, null, null, null, null, null, null },
                    { 49, null, "Direct", "Cartagena", null, "CO", null, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_48_20250829", "direct", 487, null, null, null, null, null, null, null },
                    { 50, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_49_20250913", "googleads", 215, null, null, null, null, null, null, null },
                    { 51, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 6, "/Account/Cart", null, null, "session_50_20250828", "socialmedia", 513, null, null, null, null, null, null, null },
                    { 52, null, "Facebook Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_51_20250911", "facebookads", 240, null, null, null, null, null, null, null },
                    { 53, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_52_20250827", "socialmedia", 538, null, null, null, null, null, null, null },
                    { 54, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_53_20250910", "facebookads", 266, null, null, null, null, null, null, null },
                    { 55, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_54_20250825", "socialmedia", 564, null, null, null, null, null, null, null },
                    { 56, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products", null, null, "session_55_20250909", "facebookads", 291, null, null, null, null, null, null, null },
                    { 57, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_56_20250824", "socialmedia", 589, null, null, null, null, null, null, null },
                    { 58, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products/Gaming", null, null, "session_57_20250907", "emailmarketing", 317, null, null, null, null, null, null, null },
                    { 59, null, "SEO", "Bogotá", 899572m, "CO", null, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, null, "organic", 1, "/", null, null, "session_58_20250922", "seo", 45, null, null, null, null, null, null, null },
                    { 60, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products/Gaming", null, null, "session_59_20250906", "emailmarketing", 343, null, null, null, null, null, null, null },
                    { 61, null, "SEO", "Bogotá", 1065519m, "CO", null, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, null, "organic", 1, "/", null, null, "session_60_20250920", "seo", 70, null, null, null, null, null, null, null },
                    { 62, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 5, "/Products/Gaming", null, null, "session_61_20250905", "emailmarketing", 368, null, null, null, null, null, null, null },
                    { 63, null, "SEO", "Bogotá", null, "CO", null, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "organic", 1, "/", null, null, "session_62_20250919", "seo", 96, null, null, null, null, null, null, null },
                    { 64, null, "Email Marketing", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 5, "/Products/Gaming", null, null, "session_63_20250903", "emailmarketing", 394, null, null, null, null, null, null, null },
                    { 65, null, "SEO", "Bogotá", null, "CO", null, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "organic", 2, "/", null, null, "session_64_20250918", "seo", 122, null, null, null, null, null, null, null },
                    { 66, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 5, "/Products/Gaming", null, null, "session_65_20250902", "direct", 419, null, null, null, null, null, null, null },
                    { 67, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/", null, null, "session_66_20250916", "googleads", 147, null, null, null, null, null, null, null },
                    { 68, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Products/Gaming", null, null, "session_67_20250901", "direct", 445, null, null, null, null, null, null, null },
                    { 69, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/Products", null, null, "session_68_20250915", "googleads", 173, null, null, null, null, null, null, null },
                    { 70, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_69_20250830", "direct", 470, null, null, null, null, null, null, null },
                    { 71, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_70_20250914", "googleads", 198, null, null, null, null, null, null, null },
                    { 72, null, "Direct", "Cartagena", null, "CO", null, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_71_20250829", "direct", 496, null, null, null, null, null, null, null },
                    { 73, null, "Facebook Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_72_20250912", "facebookads", 224, null, null, null, null, null, null, null },
                    { 74, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_73_20250828", "socialmedia", 522, null, null, null, null, null, null, null },
                    { 75, null, "Facebook Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_74_20250911", "facebookads", 249, null, null, null, null, null, null, null },
                    { 76, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_75_20250826", "socialmedia", 547, null, null, null, null, null, null, null },
                    { 77, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 4, "/Products", null, null, "session_76_20250910", "facebookads", 275, null, null, null, null, null, null, null },
                    { 78, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_77_20250825", "socialmedia", 573, null, null, null, null, null, null, null },
                    { 79, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products", null, null, "session_78_20250908", "facebookads", 300, null, null, null, null, null, null, null },
                    { 80, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_79_20250824", "socialmedia", 598, null, null, null, null, null, null, null },
                    { 81, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products/Gaming", null, null, "session_80_20250907", "emailmarketing", 326, null, null, null, null, null, null, null },
                    { 82, null, "SEO", "Bogotá", 957966m, "CO", null, new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, null, "organic", 1, "/", null, null, "session_81_20250921", "seo", 54, null, null, null, null, null, null, null },
                    { 83, null, "Email Marketing", "Cali", null, "CO", null, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 4, "/Products/Gaming", null, null, "session_82_20250906", "emailmarketing", 352, null, null, null, null, null, null, null },
                    { 84, null, "SEO", "Bogotá", null, "CO", null, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "organic", 1, "/", null, null, "session_83_20250920", "seo", 79, null, null, null, null, null, null, null },
                    { 85, null, "Email Marketing", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 5, "/Products/Gaming", null, null, "session_84_20250904", "emailmarketing", 377, null, null, null, null, null, null, null },
                    { 86, null, "SEO", "Bogotá", null, "CO", null, new DateTime(2025, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "organic", 1, "/", null, null, "session_85_20250919", "seo", 105, null, null, null, null, null, null, null },
                    { 87, null, "Email Marketing", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 5, "/Products/Gaming", null, null, "session_86_20250903", "emailmarketing", 403, null, null, null, null, null, null, null },
                    { 88, null, "Google Ads", "Bogotá", null, "CO", null, new DateTime(2025, 9, 17, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/", null, null, "session_87_20250917", "googleads", 131, null, null, null, null, null, null, null },
                    { 89, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 5, "/Products/Gaming", null, null, "session_88_20250902", "direct", 428, null, null, null, null, null, null, null },
                    { 90, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/", null, null, "session_89_20250916", "googleads", 156, null, null, null, null, null, null, null },
                    { 91, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Products/Gaming", null, null, "session_90_20250831", "direct", 454, null, null, null, null, null, null, null },
                    { 92, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 2, "/Products", null, null, "session_91_20250914", "googleads", 182, null, null, null, null, null, null, null },
                    { 93, null, "Direct", "Barranquilla", null, "CO", null, new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "none", 6, "/Account/Cart", null, null, "session_92_20250830", "direct", 479, null, null, null, null, null, null, null },
                    { 94, null, "Google Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_93_20250913", "googleads", 207, null, null, null, null, null, null, null },
                    { 95, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 6, "/Account/Cart", null, null, "session_94_20250828", "socialmedia", 505, null, null, null, null, null, null, null },
                    { 96, null, "Facebook Ads", "Medellín", null, "CO", null, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_95_20250912", "facebookads", 233, null, null, null, null, null, null, null },
                    { 97, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_96_20250827", "socialmedia", 531, null, null, null, null, null, null, null },
                    { 98, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 3, "/Products", null, null, "session_97_20250910", "facebookads", 258, null, null, null, null, null, null, null },
                    { 99, null, "Social Media", "Cartagena", null, "CO", null, new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "cpc", 7, "/Account/Cart", null, null, "session_98_20250826", "socialmedia", 556, null, null, null, null, null, null, null },
                    { 100, null, "Facebook Ads", "Cali", null, "CO", null, new DateTime(2025, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, "cpc", 4, "/Products", null, null, "session_99_20250909", "facebookads", 284, null, null, null, null, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_Canal",
                table: "TrafficEvents",
                column: "Canal");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_IsConversion",
                table: "TrafficEvents",
                column: "IsConversion");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_SessionId",
                table: "TrafficEvents",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingChannelMetrics_Canal_Fecha",
                table: "MarketingChannelMetrics",
                columns: new[] { "Canal", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoMarketingSources_Canal",
                table: "PedidoMarketingSources",
                column: "Canal");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoMarketingSources_PedidoId",
                table: "PedidoMarketingSources",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoMarketingSources_SessionId",
                table: "PedidoMarketingSources",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrafficEvents_Pedidos_PedidoId",
                table: "TrafficEvents",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
