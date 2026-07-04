using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SVSim.Database.Migrations
{
    /// <inheritdoc />
    public partial class ConfigViewerOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Info_ChallengeTwoPickSleeveId",
                table: "Viewers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "Info_IsSkipGachaEffect",
                table: "Viewers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Info_UseChallengeTwoPickPremiumCard",
                table: "Viewers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Info_ChallengeTwoPickSleeveId",
                table: "Viewers");

            migrationBuilder.DropColumn(
                name: "Info_IsSkipGachaEffect",
                table: "Viewers");

            migrationBuilder.DropColumn(
                name: "Info_UseChallengeTwoPickPremiumCard",
                table: "Viewers");
        }
    }
}
