using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SiteSettingsLookupsContent : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "TopicLookupValueId",
                table: "Inquiries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContentBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupValues_LookupCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "LookupCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListings_Slug",
                table: "PropertyListings",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_Slug",
                table: "PortfolioProjects",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_TopicLookupValueId",
                table: "Inquiries",
                column: "TopicLookupValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlocks_Slug",
                table: "ContentBlocks",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LookupCategories_Code",
                table: "LookupCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LookupValues_CategoryId_Code",
                table: "LookupValues",
                columns: new[] { "CategoryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_Key",
                table: "SiteSettings",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_LookupValues_TopicLookupValueId",
                table: "Inquiries",
                column: "TopicLookupValueId",
                principalTable: "LookupValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_LookupValues_TopicLookupValueId",
                table: "Inquiries");

            migrationBuilder.DropTable(
                name: "ContentBlocks");

            migrationBuilder.DropTable(
                name: "LookupValues");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "LookupCategories");

            migrationBuilder.DropIndex(
                name: "IX_PropertyListings_Slug",
                table: "PropertyListings");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioProjects_Slug",
                table: "PortfolioProjects");

            migrationBuilder.DropIndex(
                name: "IX_Inquiries_TopicLookupValueId",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "TopicLookupValueId",
                table: "Inquiries");

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
    }
}
