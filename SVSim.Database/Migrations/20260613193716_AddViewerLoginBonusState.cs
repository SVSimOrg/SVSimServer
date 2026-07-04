using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddViewerLoginBonusState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyLoginBonuses");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginBonusClaimedAt",
                table: "Viewers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoginBonusStreak",
                table: "Viewers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginBonusClaimedAt",
                table: "Viewers");

            migrationBuilder.DropColumn(
                name: "LoginBonusStreak",
                table: "Viewers");

            migrationBuilder.CreateTable(
                name: "DailyLoginBonuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    BonusData = table.Column<string>(type: "jsonb", nullable: false),
                    BonusId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyLoginBonuses", x => x.Id);
                });
        }
    }
}
