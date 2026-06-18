using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(Avenue804.Web.Data.ApplicationDbContext))]
    [Migration("20260511000000_PropertyCategoryAndType")]
    public partial class PropertyCategoryAndType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emirates",
                table: "PropertyListings",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyCategory",
                table: "PropertyListings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyType",
                table: "PropertyListings",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emirates",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "PropertyCategory",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "PropertyType",
                table: "PropertyListings");
        }
    }
}
