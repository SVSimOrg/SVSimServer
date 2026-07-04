using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSleeveShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SleeveShopSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleeveShopSeries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SleeveShopProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    SeriesId = table.Column<int>(type: "integer", nullable: false),
                    NameKey = table.Column<string>(type: "text", nullable: false),
                    PriceCrystal = table.Column<int>(type: "integer", nullable: true),
                    PriceRupy = table.Column<int>(type: "integer", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleeveShopProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SleeveShopProducts_SleeveShopSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "SleeveShopSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SleeveShopProductRewardEntry",
                columns: table => new
                {
                    SleeveShopProductEntryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleeveShopProductRewardEntry", x => new { x.SleeveShopProductEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_SleeveShopProductRewardEntry_SleeveShopProducts_SleeveShopP~",
                        column: x => x.SleeveShopProductEntryId,
                        principalTable: "SleeveShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SleeveShopProducts_SeriesId",
                table: "SleeveShopProducts",
                column: "SeriesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SleeveShopProductRewardEntry");

            migrationBuilder.DropTable(
                name: "SleeveShopProducts");

            migrationBuilder.DropTable(
                name: "SleeveShopSeries");
        }
    }
}
