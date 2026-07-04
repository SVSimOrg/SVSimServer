using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddViewerPresents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewerClaimedTutorialGifts");

            migrationBuilder.CreateTable(
                name: "ViewerPresents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    PresentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    RewardDetailId = table.Column<long>(type: "bigint", nullable: false),
                    RewardCount = table.Column<long>(type: "bigint", nullable: false),
                    ConditionNumber = table.Column<int>(type: "integer", nullable: false),
                    PresentLimitType = table.Column<int>(type: "integer", nullable: false),
                    RewardLimitTime = table.Column<long>(type: "bigint", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerPresents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViewerPresents_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerPresents_ViewerId_PresentId",
                table: "ViewerPresents",
                columns: new[] { "ViewerId", "PresentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViewerPresents_ViewerId_Status_CreatedAt",
                table: "ViewerPresents",
                columns: new[] { "ViewerId", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewerPresents");

            migrationBuilder.CreateTable(
                name: "ViewerClaimedTutorialGifts",
                columns: table => new
                {
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    PresentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerClaimedTutorialGifts", x => new { x.ViewerId, x.PresentId });
                    table.ForeignKey(
                        name: "FK_ViewerClaimedTutorialGifts_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
