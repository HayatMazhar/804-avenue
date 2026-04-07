using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PropertyListingInquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyListingInquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IAmLookupValueId = table.Column<int>(type: "int", nullable: true),
                    WantToLookupValueId = table.Column<int>(type: "int", nullable: true),
                    PropertyTypeLookupValueId = table.Column<int>(type: "int", nullable: true),
                    PropertyDetailLookupValueId = table.Column<int>(type: "int", nullable: true),
                    OtherDetails = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LocationLookupValueId = table.Column<int>(type: "int", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyListingInquiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyListingInquiries_LookupValues_IAmLookupValueId",
                        column: x => x.IAmLookupValueId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PropertyListingInquiries_LookupValues_LocationLookupValueId",
                        column: x => x.LocationLookupValueId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PropertyListingInquiries_LookupValues_PropertyDetailLookupValueId",
                        column: x => x.PropertyDetailLookupValueId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PropertyListingInquiries_LookupValues_PropertyTypeLookupValueId",
                        column: x => x.PropertyTypeLookupValueId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PropertyListingInquiries_LookupValues_WantToLookupValueId",
                        column: x => x.WantToLookupValueId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListingInquiries_IAmLookupValueId",
                table: "PropertyListingInquiries",
                column: "IAmLookupValueId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListingInquiries_LocationLookupValueId",
                table: "PropertyListingInquiries",
                column: "LocationLookupValueId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListingInquiries_PropertyDetailLookupValueId",
                table: "PropertyListingInquiries",
                column: "PropertyDetailLookupValueId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListingInquiries_PropertyTypeLookupValueId",
                table: "PropertyListingInquiries",
                column: "PropertyTypeLookupValueId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListingInquiries_WantToLookupValueId",
                table: "PropertyListingInquiries",
                column: "WantToLookupValueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyListingInquiries");
        }
    }
}
