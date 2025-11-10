using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ComputerStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketingConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MarketingCosts_MarketingAccounts_MarketingAccountId",
                table: "MarketingCosts");

            migrationBuilder.DropForeignKey(
                name: "FK_MarketingCosts_MarketingCampaigns_CampaignId",
                table: "MarketingCosts");

            migrationBuilder.DropTable(
                name: "MarketingMetrics");

            migrationBuilder.DropTable(
                name: "MarketingCampaigns");

            migrationBuilder.DropTable(
                name: "MarketingAccounts");

            migrationBuilder.DropIndex(
                name: "IX_MarketingCosts_CampaignId",
                table: "MarketingCosts");

            migrationBuilder.DropIndex(
                name: "IX_MarketingCosts_MarketingAccountId",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "Conversiones",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "MarketingAccountId",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "ROAS",
                table: "MarketingCosts");

            migrationBuilder.DropColumn(
                name: "ValorConversiones",
                table: "MarketingCosts");

            migrationBuilder.CreateTable(
                name: "MarketingConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Canal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Plataforma = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConfiguracionEncriptada = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaSincronizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notas = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IntentosConexion = table.Column<int>(type: "integer", nullable: false),
                    UltimoIntento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingConfigurations", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 0, 40, 17, 423, DateTimeKind.Utc).AddTicks(9177));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 0, 40, 17, 423, DateTimeKind.Utc).AddTicks(9186));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 0, 40, 17, 423, DateTimeKind.Utc).AddTicks(9190));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 0, 40, 17, 423, DateTimeKind.Utc).AddTicks(9235));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 14, 0, 40, 17, 423, DateTimeKind.Utc).AddTicks(9239));

            migrationBuilder.CreateIndex(
                name: "IX_MarketingConfigurations_Activo",
                table: "MarketingConfigurations",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingConfigurations_Canal_Plataforma",
                table: "MarketingConfigurations",
                columns: new[] { "Canal", "Plataforma" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketingConfigurations_Estado",
                table: "MarketingConfigurations",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingConfigurations_FechaCreacion",
                table: "MarketingConfigurations",
                column: "FechaCreacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketingConfigurations");

            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "MarketingCosts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Conversiones",
                table: "MarketingCosts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "MarketingCosts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MarketingAccountId",
                table: "MarketingCosts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ROAS",
                table: "MarketingCosts",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorConversiones",
                table: "MarketingCosts",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarketingAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccessToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ConfiguracionJson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaExpiracionToken = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Plataforma = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SecretKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketingCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarketingAccountId = table.Column<int>(type: "integer", nullable: false),
                    AudienciaObjetivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CampaignIdExterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConfiguracionJson = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PalabrasClaveJson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PresupuestoDiario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PresupuestoTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TipoCampana = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingCampaigns_MarketingAccounts_MarketingAccountId",
                        column: x => x.MarketingAccountId,
                        principalTable: "MarketingAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketingMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: true),
                    MarketingAccountId = table.Column<int>(type: "integer", nullable: false),
                    CPA = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CPC = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CPM = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CTR = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Clicks = table.Column<int>(type: "integer", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Conversiones = table.Column<int>(type: "integer", nullable: false),
                    CostoTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Impresiones = table.Column<int>(type: "integer", nullable: false),
                    MetricasExtraJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ROAS = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ValorConversion = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingMetrics_MarketingAccounts_MarketingAccountId",
                        column: x => x.MarketingAccountId,
                        principalTable: "MarketingAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketingMetrics_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 13, 22, 38, 54, 926, DateTimeKind.Utc).AddTicks(89));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 13, 22, 38, 54, 926, DateTimeKind.Utc).AddTicks(98));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 13, 22, 38, 54, 926, DateTimeKind.Utc).AddTicks(102));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 13, 22, 38, 54, 926, DateTimeKind.Utc).AddTicks(106));

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 13, 22, 38, 54, 926, DateTimeKind.Utc).AddTicks(111));

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCosts_CampaignId",
                table: "MarketingCosts",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCosts_MarketingAccountId",
                table: "MarketingCosts",
                column: "MarketingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingAccounts_IsActive",
                table: "MarketingAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingAccounts_Plataforma",
                table: "MarketingAccounts",
                column: "Plataforma");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingAccounts_Plataforma_IsActive",
                table: "MarketingAccounts",
                columns: new[] { "Plataforma", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_Estado",
                table: "MarketingCampaigns",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_MarketingAccountId",
                table: "MarketingCampaigns",
                column: "MarketingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_MarketingAccountId_Estado",
                table: "MarketingCampaigns",
                columns: new[] { "MarketingAccountId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_TipoCampana",
                table: "MarketingCampaigns",
                column: "TipoCampana");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingMetrics_CampaignId",
                table: "MarketingMetrics",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingMetrics_CampaignId_Fecha",
                table: "MarketingMetrics",
                columns: new[] { "CampaignId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingMetrics_Fecha",
                table: "MarketingMetrics",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingMetrics_MarketingAccountId",
                table: "MarketingMetrics",
                column: "MarketingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingMetrics_MarketingAccountId_Fecha",
                table: "MarketingMetrics",
                columns: new[] { "MarketingAccountId", "Fecha" });

            migrationBuilder.AddForeignKey(
                name: "FK_MarketingCosts_MarketingAccounts_MarketingAccountId",
                table: "MarketingCosts",
                column: "MarketingAccountId",
                principalTable: "MarketingAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MarketingCosts_MarketingCampaigns_CampaignId",
                table: "MarketingCosts",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
