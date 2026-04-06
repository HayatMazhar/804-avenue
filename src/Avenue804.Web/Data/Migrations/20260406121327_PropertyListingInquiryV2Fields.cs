using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PropertyListingInquiryV2Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetLookupValueId",
                table: "PropertyListingInquiries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpectedMoveInDate",
                table: "PropertyListingInquiries",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequirementDetails",
                table: "PropertyListingInquiries",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListingInquiries_BudgetLookupValueId",
                table: "PropertyListingInquiries",
                column: "BudgetLookupValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyListingInquiries_LookupValues_BudgetLookupValueId",
                table: "PropertyListingInquiries",
                column: "BudgetLookupValueId",
                principalTable: "LookupValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyListingInquiries_LookupValues_BudgetLookupValueId",
                table: "PropertyListingInquiries");

            migrationBuilder.DropIndex(
                name: "IX_PropertyListingInquiries_BudgetLookupValueId",
                table: "PropertyListingInquiries");

            migrationBuilder.DropColumn(
                name: "BudgetLookupValueId",
                table: "PropertyListingInquiries");

            migrationBuilder.DropColumn(
                name: "ExpectedMoveInDate",
                table: "PropertyListingInquiries");

            migrationBuilder.DropColumn(
                name: "RequirementDetails",
                table: "PropertyListingInquiries");
        }
    }
}
