using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class OffPlanProjectFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmenitiesDescription",
                table: "PropertyListings",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BathroomOptions",
                table: "PropertyListings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BedroomOptions",
                table: "PropertyListings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DownPaymentPercent",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DuringConstructionPercent",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyFeatures",
                table: "PropertyListings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NearbyLandmarks",
                table: "PropertyListings",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnHandoverPercent",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectAddress",
                table: "PropertyListings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectStatus",
                table: "PropertyListings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartingSizeSqft",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "PropertyListings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalBuildings",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalFloors",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalUnits",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitTypes",
                table: "PropertyListings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmenitiesDescription",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "BathroomOptions",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "BedroomOptions",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "DownPaymentPercent",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "DuringConstructionPercent",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "KeyFeatures",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "NearbyLandmarks",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "OnHandoverPercent",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "ProjectAddress",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "ProjectStatus",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "StartingSizeSqft",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "TotalBuildings",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "TotalFloors",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "TotalUnits",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "UnitTypes",
                table: "PropertyListings");
        }
    }
}
