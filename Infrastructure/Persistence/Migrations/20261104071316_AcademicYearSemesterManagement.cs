using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AcademicYearSemesterManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SemesterId",
                table: "Exams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "Enrollments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Semesters_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Exams_SemesterId", table: "Exams", column: "SemesterId");
            migrationBuilder.CreateIndex(name: "IX_Enrollments_AcademicYearId", table: "Enrollments", column: "AcademicYearId");
            migrationBuilder.CreateIndex(name: "IX_Enrollments_StudentId_AcademicYearId", table: "Enrollments", columns: new[] { "StudentId", "AcademicYearId" });
            migrationBuilder.CreateIndex(name: "IX_Classes_AcademicYearId", table: "Classes", column: "AcademicYearId");
            migrationBuilder.CreateIndex(name: "IX_AcademicYears_Name", table: "AcademicYears", column: "Name", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Semesters_AcademicYearId", table: "Semesters", column: "AcademicYearId");
            migrationBuilder.CreateIndex(name: "IX_Semesters_AcademicYearId_Name", table: "Semesters", columns: new[] { "AcademicYearId", "Name" }, unique: true);

            migrationBuilder.AddForeignKey(name: "FK_Classes_AcademicYears_AcademicYearId", table: "Classes", column: "AcademicYearId", principalTable: "AcademicYears", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Enrollments_AcademicYears_AcademicYearId", table: "Enrollments", column: "AcademicYearId", principalTable: "AcademicYears", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Exams_Semesters_SemesterId", table: "Exams", column: "SemesterId", principalTable: "Semesters", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Classes_AcademicYears_AcademicYearId", table: "Classes");
            migrationBuilder.DropForeignKey(name: "FK_Enrollments_AcademicYears_AcademicYearId", table: "Enrollments");
            migrationBuilder.DropForeignKey(name: "FK_Exams_Semesters_SemesterId", table: "Exams");
            migrationBuilder.DropTable(name: "Semesters");
            migrationBuilder.DropTable(name: "AcademicYears");
            migrationBuilder.DropIndex(name: "IX_Exams_SemesterId", table: "Exams");
            migrationBuilder.DropIndex(name: "IX_Enrollments_AcademicYearId", table: "Enrollments");
            migrationBuilder.DropIndex(name: "IX_Enrollments_StudentId_AcademicYearId", table: "Enrollments");
            migrationBuilder.DropIndex(name: "IX_Classes_AcademicYearId", table: "Classes");
            migrationBuilder.DropColumn(name: "SemesterId", table: "Exams");
            migrationBuilder.DropColumn(name: "AcademicYearId", table: "Enrollments");
            migrationBuilder.DropColumn(name: "EndDate", table: "Enrollments");
            migrationBuilder.DropColumn(name: "StartDate", table: "Enrollments");
            migrationBuilder.DropColumn(name: "Status", table: "Enrollments");
            migrationBuilder.DropColumn(name: "AcademicYearId", table: "Classes");
        }
    }
}
