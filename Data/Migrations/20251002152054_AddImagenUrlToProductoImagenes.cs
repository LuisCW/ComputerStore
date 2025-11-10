using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImagenUrlToProductoImagenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "ProductoImagenes",
                newName: "ImagenUrl");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "ProductoImagenes",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreArchivo",
                table: "ProductoImagenes",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4480));

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
                column: "CreatedDate",
                value: new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4499));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4505));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 22, 15, 20, 53, 474, DateTimeKind.Utc).AddTicks(4513));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreArchivo",
                table: "ProductoImagenes");

            migrationBuilder.RenameColumn(
                name: "ImagenUrl",
                table: "ProductoImagenes",
                newName: "Url");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "ProductoImagenes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 20, 22, 51, 24, 77, DateTimeKind.Utc).AddTicks(4233));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 20, 22, 51, 24, 77, DateTimeKind.Utc).AddTicks(4296));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 20, 22, 51, 24, 77, DateTimeKind.Utc).AddTicks(4301));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 20, 22, 51, 24, 77, DateTimeKind.Utc).AddTicks(4306));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 20, 22, 51, 24, 77, DateTimeKind.Utc).AddTicks(4311));
        }
    }
}
