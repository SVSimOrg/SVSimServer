using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildInviteSurrogatePk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildInvites",
                table: "GuildInvites");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "GuildInvites",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildInvites",
                table: "GuildInvites",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GuildInvites_GuildId_InviteeViewerId",
                table: "GuildInvites",
                columns: new[] { "GuildId", "InviteeViewerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildInvites",
                table: "GuildInvites");

            migrationBuilder.DropIndex(
                name: "IX_GuildInvites_GuildId_InviteeViewerId",
                table: "GuildInvites");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GuildInvites");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildInvites",
                table: "GuildInvites",
                columns: new[] { "GuildId", "InviteeViewerId" });
        }
    }
}
