using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SlugUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PropertyListings_Slug",
                table: "PropertyListings");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_Slug",
                table: "PortfolioProjects");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListings_Slug",
                table: "PropertyListings",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_Slug",
                table: "PortfolioProjects",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_Slug",
                table: "PortfolioProjects");

            migrationBuilder.DropIndex(
                name: "IX_PropertyListings_Slug",
                table: "PropertyListings");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListings_Slug",
                table: "PropertyListings",
                column: "Slug");
        }
    }
}
