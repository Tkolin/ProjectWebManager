using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.Migrations
{
    /// <inheritdoc />
    public partial class gsg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateEnd",
                table: "Project",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<int>(type: "int", unicode: false, maxLength: 255, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DocumentsToProject",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsToProject", x => new { x.ProjectId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_DocumentToProject_Document",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentToProject_Project",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsToProject_DocumentId",
                table: "DocumentsToProject",
                column: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentsToProject");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateEnd",
                table: "Project",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");
        }
    }
}
