using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Practise.Migrations
{
    /// <inheritdoc />
    public partial class AddTotpAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTotpEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TotpSecretKey",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTotpEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotpSecretKey",
                table: "Users");
        }
    }
}
