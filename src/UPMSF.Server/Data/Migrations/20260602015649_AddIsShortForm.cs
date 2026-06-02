using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPMSF.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsShortForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShortForm",
                table: "Applications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShortForm",
                table: "Applications");
        }
    }
}
