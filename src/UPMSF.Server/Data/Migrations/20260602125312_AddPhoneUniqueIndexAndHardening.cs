using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPMSF.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneUniqueIndexAndHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applicants_Phone_RegistrationId",
                table: "Applicants");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Phone",
                table: "Applicants",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applicants_Phone",
                table: "Applicants");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Phone_RegistrationId",
                table: "Applicants",
                columns: new[] { "Phone", "RegistrationId" });
        }
    }
}
