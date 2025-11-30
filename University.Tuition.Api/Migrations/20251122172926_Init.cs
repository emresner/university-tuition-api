using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace University.Tuition.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TuitionCharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuitionCharges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TuitionCharges_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "FullName", "StudentNo" },
                values: new object[,]
                {
                    { 1, "Selin Yılmaz", "20201234" },
                    { 2, "Ahmet Demir", "20204567" }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "Amount", "CreatedAt", "StudentId", "Term" },
                values: new object[] { 1, 3000.00m, new DateTime(2025, 11, 22, 17, 29, 26, 400, DateTimeKind.Utc).AddTicks(1862), 1, "2025-Spring" });

            migrationBuilder.InsertData(
                table: "TuitionCharges",
                columns: new[] { "Id", "Amount", "CreatedAt", "StudentId", "Term" },
                values: new object[,]
                {
                    { 1, 12000.00m, new DateTime(2025, 11, 22, 17, 29, 26, 400, DateTimeKind.Utc).AddTicks(1847), 1, "2025-Spring" },
                    { 2, 9000.00m, new DateTime(2025, 11, 22, 17, 29, 26, 400, DateTimeKind.Utc).AddTicks(1850), 2, "2025-Spring" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId_Term",
                table: "Payments",
                columns: new[] { "StudentId", "Term" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNo",
                table: "Students",
                column: "StudentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuitionCharges_StudentId_Term",
                table: "TuitionCharges",
                columns: new[] { "StudentId", "Term" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "TuitionCharges");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
