using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeveloperProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwardsRecognition",
                table: "Developers",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mission",
                table: "Developers",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Philosophy",
                table: "Developers",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalProjects",
                table: "Developers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitsDelivered",
                table: "Developers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vision",
                table: "Developers",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsInBusiness",
                table: "Developers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardsRecognition",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "Mission",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "Philosophy",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "TotalProjects",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "UnitsDelivered",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "Vision",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "YearsInBusiness",
                table: "Developers");
        }
    }
}
