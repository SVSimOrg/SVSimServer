using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaderSkinShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaderSkinShopSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SetSalesStatus = table.Column<int>(type: "integer", nullable: false),
                    SetPriceCrystal = table.Column<int>(type: "integer", nullable: true),
                    SetPriceRupy = table.Column<int>(type: "integer", nullable: true),
                    SetPriceTicket = table.Column<int>(type: "integer", nullable: true),
                    SetPriceTicketId = table.Column<long>(type: "bigint", nullable: true),
                    SetCompletionRewardStatus = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderSkinShopSeries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerLeaderSkinSetClaims",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    SeriesId = table.Column<int>(type: "integer", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerLeaderSkinSetClaims", x => new { x.ViewerId, x.SeriesId });
                });

            migrationBuilder.CreateTable(
                name: "LeaderSkinShopProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    SeriesId = table.Column<int>(type: "integer", nullable: false),
                    LeaderSkinId = table.Column<int>(type: "integer", nullable: false),
                    ProductNameKey = table.Column<string>(type: "text", nullable: false),
                    IntroductionKey = table.Column<string>(type: "text", nullable: false),
                    CvNameKey = table.Column<string>(type: "text", nullable: false),
                    SinglePriceCrystal = table.Column<int>(type: "integer", nullable: true),
                    SinglePriceRupy = table.Column<int>(type: "integer", nullable: true),
                    SinglePriceTicket = table.Column<int>(type: "integer", nullable: true),
                    TicketNumber = table.Column<int>(type: "integer", nullable: true),
                    TicketItemId = table.Column<long>(type: "bigint", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderSkinShopProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderSkinShopProducts_LeaderSkinShopSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "LeaderSkinShopSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderSkinShopSeriesRewardEntry",
                columns: table => new
                {
                    LeaderSkinShopSeriesEntryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderSkinShopSeriesRewardEntry", x => new { x.LeaderSkinShopSeriesEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_LeaderSkinShopSeriesRewardEntry_LeaderSkinShopSeries_Leader~",
                        column: x => x.LeaderSkinShopSeriesEntryId,
                        principalTable: "LeaderSkinShopSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderSkinShopProductRewardEntry",
                columns: table => new
                {
                    LeaderSkinShopProductEntryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderSkinShopProductRewardEntry", x => new { x.LeaderSkinShopProductEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_LeaderSkinShopProductRewardEntry_LeaderSkinShopProducts_Lea~",
                        column: x => x.LeaderSkinShopProductEntryId,
                        principalTable: "LeaderSkinShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderSkinShopProducts_SeriesId",
                table: "LeaderSkinShopProducts",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewerLeaderSkinSetClaims_ViewerId",
                table: "ViewerLeaderSkinSetClaims",
                column: "ViewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderSkinShopProductRewardEntry");

            migrationBuilder.DropTable(
                name: "LeaderSkinShopSeriesRewardEntry");

            migrationBuilder.DropTable(
                name: "ViewerLeaderSkinSetClaims");

            migrationBuilder.DropTable(
                name: "LeaderSkinShopProducts");

            migrationBuilder.DropTable(
                name: "LeaderSkinShopSeries");
        }
    }
}
