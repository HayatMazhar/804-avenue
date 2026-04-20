using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase2Properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmenitiesJson",
                table: "PropertyListings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletionPercent",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeveloperId",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloorPlanUrl",
                table: "PropertyListings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HandoverDate",
                table: "PropertyListings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOffPlan",
                table: "PropertyListings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentPlan",
                table: "PropertyListings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousPrice",
                table: "PropertyListings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VirtualTourUrl",
                table: "PropertyListings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AreaGuides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Emirate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HeroImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Overview = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: true),
                    AvgPriceSaleSqft = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AvgRentYearly = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PopularWith = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NearbyLandmarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SchoolsNearby = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TransportLinks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaGuides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Developers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Headquarters = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstablishedYear = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListings_DeveloperId",
                table: "PropertyListings",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaGuides_Slug",
                table: "AreaGuides",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Developers_Slug",
                table: "Developers",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyListings_Developers_DeveloperId",
                table: "PropertyListings",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyListings_Developers_DeveloperId",
                table: "PropertyListings");

            migrationBuilder.DropTable(
                name: "AreaGuides");

            migrationBuilder.DropTable(
                name: "Developers");

            migrationBuilder.DropIndex(
                name: "IX_PropertyListings_DeveloperId",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "AmenitiesJson",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "CompletionPercent",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "DeveloperId",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "FloorPlanUrl",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "HandoverDate",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "IsOffPlan",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "PaymentPlan",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "PreviousPrice",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "VirtualTourUrl",
                table: "PropertyListings");
        }
    }
}
