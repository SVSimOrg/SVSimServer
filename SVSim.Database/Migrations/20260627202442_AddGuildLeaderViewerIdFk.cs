using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildLeaderViewerIdFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "LeaderViewerId",
                table: "Guilds",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_LeaderViewerId",
                table: "Guilds",
                column: "LeaderViewerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_Viewers_LeaderViewerId",
                table: "Guilds",
                column: "LeaderViewerId",
                principalTable: "Viewers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Viewers_LeaderViewerId",
                table: "Guilds");

            migrationBuilder.DropIndex(
                name: "IX_Guilds_LeaderViewerId",
                table: "Guilds");

            migrationBuilder.AlterColumn<long>(
                name: "LeaderViewerId",
                table: "Guilds",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
