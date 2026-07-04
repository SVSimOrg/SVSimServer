using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddViewerFreePackClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewerFreePackClaim",
                columns: table => new
                {
                    FreeGachaCampaignId = table.Column<int>(type: "integer", nullable: false),
                    ViewerId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimCount = table.Column<int>(type: "integer", nullable: false),
                    LastClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerFreePackClaim", x => new { x.ViewerId, x.FreeGachaCampaignId });
                    table.ForeignKey(
                        name: "FK_ViewerFreePackClaim_Viewers_ViewerId",
                        column: x => x.ViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewerFreePackClaim");
        }
    }
}
