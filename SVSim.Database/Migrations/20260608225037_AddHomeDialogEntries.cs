using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeDialogEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeDialogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    TitleTextId = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: false),
                    ButtonListJson = table.Column<string>(type: "jsonb", nullable: false),
                    BeginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeDialogEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeDialogEntries_BeginTime_EndTime",
                table: "HomeDialogEntries",
                columns: new[] { "BeginTime", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HomeDialogEntries_BeginTime_EndTime",
                table: "HomeDialogEntries");

            migrationBuilder.DropTable(
                name: "HomeDialogEntries");
        }
    }
}
