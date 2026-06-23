using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CommunityLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "AreaGuides",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attractions",
                table: "AreaGuides",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRoiPercent",
                table: "AreaGuides",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AreaGuides",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dining",
                table: "AreaGuides",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "AreaGuides",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Healthcare",
                table: "AreaGuides",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestmentInsights",
                table: "AreaGuides",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifestyleServices",
                table: "AreaGuides",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RentalYieldPercent",
                table: "AreaGuides",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shopping",
                table: "AreaGuides",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "AreaGuides",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transportation",
                table: "AreaGuides",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "Attractions",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "AverageRoiPercent",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "Dining",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "Healthcare",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "InvestmentInsights",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "LifestyleServices",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "RentalYieldPercent",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "Shopping",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "AreaGuides");

            migrationBuilder.DropColumn(
                name: "Transportation",
                table: "AreaGuides");
        }
    }
}
