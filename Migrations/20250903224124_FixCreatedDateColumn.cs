using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerStore.Migrations
{
    /// <inheritdoc />
    public partial class FixCreatedDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Productos",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "Productos",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "OriginalPrice" },
                values: new object[] { new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6100), null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "OriginalPrice" },
                values: new object[] { new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6107), null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "OriginalPrice" },
                values: new object[] { new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6112), null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "OriginalPrice" },
                values: new object[] { new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6117), null });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "OriginalPrice" },
                values: new object[] { new DateTime(2025, 9, 3, 22, 41, 23, 231, DateTimeKind.Utc).AddTicks(6122), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Productos");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Productos",
                newName: "FechaCreacion");

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
    }
}
