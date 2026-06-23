using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class OwnerRequestPropertyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "OwnerListingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Emirate",
                table: "OwnerListingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IAmRole",
                table: "OwnerListingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyCategory",
                table: "OwnerListingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertySubType",
                table: "OwnerListingRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "OwnerListingRequests");

            migrationBuilder.DropColumn(
                name: "Emirate",
                table: "OwnerListingRequests");

            migrationBuilder.DropColumn(
                name: "IAmRole",
                table: "OwnerListingRequests");

            migrationBuilder.DropColumn(
                name: "PropertyCategory",
                table: "OwnerListingRequests");

            migrationBuilder.DropColumn(
                name: "PropertySubType",
                table: "OwnerListingRequests");
        }
    }
}
