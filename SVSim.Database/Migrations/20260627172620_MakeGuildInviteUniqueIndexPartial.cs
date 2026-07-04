using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class MakeGuildInviteUniqueIndexPartial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuildInvites_GuildId_InviteeViewerId",
                table: "GuildInvites");

            migrationBuilder.CreateIndex(
                name: "IX_GuildInvites_GuildId_InviteeViewerId",
                table: "GuildInvites",
                columns: new[] { "GuildId", "InviteeViewerId" },
                unique: true,
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuildInvites_GuildId_InviteeViewerId",
                table: "GuildInvites");

            migrationBuilder.CreateIndex(
                name: "IX_GuildInvites_GuildId_InviteeViewerId",
                table: "GuildInvites",
                columns: new[] { "GuildId", "InviteeViewerId" },
                unique: true);
        }
    }
}
