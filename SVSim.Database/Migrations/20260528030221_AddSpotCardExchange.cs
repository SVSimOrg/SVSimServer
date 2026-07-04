using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSpotCardExchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Currency_SpotPoints",
                table: "Viewers",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SpotCardExchangeCatalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    ExchangePoint = table.Column<int>(type: "integer", nullable: false),
                    TsRotationId = table.Column<long>(type: "bigint", nullable: false),
                    IsPreRelease = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotCardExchangeCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerSpotCardExchanges",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    IsPreRelease = table.Column<bool>(type: "boolean", nullable: false),
                    ExchangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerSpotCardExchanges", x => new { x.ViewerId, x.CardId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerSpotCardExchanges_ViewerId",
                table: "ViewerSpotCardExchanges",
                column: "ViewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpotCardExchangeCatalog");

            migrationBuilder.DropTable(
                name: "ViewerSpotCardExchanges");

            migrationBuilder.DropColumn(
                name: "Currency_SpotPoints",
                table: "Viewers");
        }
    }
}
