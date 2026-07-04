using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildDeck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildDeckSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    NameKey = table.Column<string>(type: "text", nullable: false),
                    IntroKey = table.Column<string>(type: "text", nullable: false),
                    TitlePath = table.Column<string>(type: "text", nullable: false),
                    DrumrollPath = table.Column<string>(type: "text", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildDeckSeries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewerBuildDeckProductPurchase",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerBuildDeckProductPurchase", x => new { x.ViewerId, x.Id });
                    table.ForeignKey(
                        name: "FK_ViewerBuildDeckProductPurchase_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildDeckProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    SeriesId = table.Column<int>(type: "integer", nullable: false),
                    LeaderId = table.Column<int>(type: "integer", nullable: false),
                    DeckCode = table.Column<string>(type: "text", nullable: false),
                    ProductNameKey = table.Column<string>(type: "text", nullable: false),
                    FeaturedCardId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseNumMax = table.Column<int>(type: "integer", nullable: false),
                    IntroPriceCrystal = table.Column<int>(type: "integer", nullable: true),
                    RegularPriceCrystal = table.Column<int>(type: "integer", nullable: true),
                    IntroPriceRupy = table.Column<int>(type: "integer", nullable: true),
                    RegularPriceRupy = table.Column<int>(type: "integer", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildDeckProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildDeckProducts_BuildDeckSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "BuildDeckSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildDeckSeriesRewardEntry",
                columns: table => new
                {
                    BuildDeckSeriesEntryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TierIndex = table.Column<int>(type: "integer", nullable: false),
                    ItemIndex = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false),
                    MessageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildDeckSeriesRewardEntry", x => new { x.BuildDeckSeriesEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_BuildDeckSeriesRewardEntry_BuildDeckSeries_BuildDeckSeriesE~",
                        column: x => x.BuildDeckSeriesEntryId,
                        principalTable: "BuildDeckSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildDeckProductCardEntry",
                columns: table => new
                {
                    BuildDeckProductEntryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    IsSpot = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildDeckProductCardEntry", x => new { x.BuildDeckProductEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_BuildDeckProductCardEntry_BuildDeckProducts_BuildDeckProduc~",
                        column: x => x.BuildDeckProductEntryId,
                        principalTable: "BuildDeckProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildDeckProductRewardEntry",
                columns: table => new
                {
                    BuildDeckProductEntryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RewardIndex = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardNumber = table.Column<int>(type: "integer", nullable: false),
                    MessageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildDeckProductRewardEntry", x => new { x.BuildDeckProductEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_BuildDeckProductRewardEntry_BuildDeckProducts_BuildDeckProd~",
                        column: x => x.BuildDeckProductEntryId,
                        principalTable: "BuildDeckProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildDeckProducts_SeriesId",
                table: "BuildDeckProducts",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewerBuildDeckProductPurchase_ViewerId_ProductId",
                table: "ViewerBuildDeckProductPurchase",
                columns: new[] { "ViewerId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildDeckProductCardEntry");

            migrationBuilder.DropTable(
                name: "BuildDeckProductRewardEntry");

            migrationBuilder.DropTable(
                name: "BuildDeckSeriesRewardEntry");

            migrationBuilder.DropTable(
                name: "ViewerBuildDeckProductPurchase");

            migrationBuilder.DropTable(
                name: "BuildDeckProducts");

            migrationBuilder.DropTable(
                name: "BuildDeckSeries");
        }
    }
}
