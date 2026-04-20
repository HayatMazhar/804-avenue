using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avenue804.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase3Features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectProgressItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClientEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProjectTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    StageNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    EstimatedCompletion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectProgressItems", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectProgressItems");
        }
    }
}
