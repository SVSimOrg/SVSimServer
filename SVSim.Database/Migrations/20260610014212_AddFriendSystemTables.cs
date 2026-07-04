using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendSystemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewerFriendApplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromViewerId = table.Column<long>(type: "bigint", nullable: false),
                    ToViewerId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MissionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerFriendApplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViewerFriendApplies_Viewers_FromViewerId",
                        column: x => x.FromViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ViewerFriendApplies_Viewers_ToViewerId",
                        column: x => x.ToViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewerFriends",
                columns: table => new
                {
                    OwnerViewerId = table.Column<long>(type: "bigint", nullable: false),
                    FriendViewerId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerFriends", x => new { x.OwnerViewerId, x.FriendViewerId });
                    table.ForeignKey(
                        name: "FK_ViewerFriends_Viewers_FriendViewerId",
                        column: x => x.FriendViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ViewerFriends_Viewers_OwnerViewerId",
                        column: x => x.OwnerViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewerPlayedTogethers",
                columns: table => new
                {
                    OwnerViewerId = table.Column<long>(type: "bigint", nullable: false),
                    OpponentViewerId = table.Column<long>(type: "bigint", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlayedMode = table.Column<int>(type: "integer", nullable: false),
                    BattleType = table.Column<int>(type: "integer", nullable: false),
                    DeckFormat = table.Column<int>(type: "integer", nullable: false),
                    TwoPickType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerPlayedTogethers", x => new { x.OwnerViewerId, x.OpponentViewerId });
                    table.ForeignKey(
                        name: "FK_ViewerPlayedTogethers_Viewers_OwnerViewerId",
                        column: x => x.OwnerViewerId,
                        principalTable: "Viewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerFriendApplies_FromViewerId_ToViewerId",
                table: "ViewerFriendApplies",
                columns: new[] { "FromViewerId", "ToViewerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViewerFriendApplies_ToViewerId",
                table: "ViewerFriendApplies",
                column: "ToViewerId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewerFriends_FriendViewerId",
                table: "ViewerFriends",
                column: "FriendViewerId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewerFriends_OwnerViewerId_CreatedAt",
                table: "ViewerFriends",
                columns: new[] { "OwnerViewerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerPlayedTogethers_OwnerViewerId_PlayedAt",
                table: "ViewerPlayedTogethers",
                columns: new[] { "OwnerViewerId", "PlayedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewerFriendApplies");

            migrationBuilder.DropTable(
                name: "ViewerFriends");

            migrationBuilder.DropTable(
                name: "ViewerPlayedTogethers");
        }
    }
}
