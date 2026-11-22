using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TimeTableFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaseTimetables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    SchoolDayStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakAfterPeriodNumber = table.Column<int>(type: "int", nullable: true),
                    BreakDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseTimetables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseTimetables_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HolidayExceptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayExceptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseTimetableEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseTimetableId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseTimetableEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseTimetableEntries_BaseTimetables_BaseTimetableId",
                        column: x => x.BaseTimetableId,
                        principalTable: "BaseTimetables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaseTimetableEntries_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaseTimetableEntries_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BaseTimetablePeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseTimetableId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsBreak = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseTimetablePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseTimetablePeriods_BaseTimetables_BaseTimetableId",
                        column: x => x.BaseTimetableId,
                        principalTable: "BaseTimetables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeeklySchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceBaseTimetableId = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklySchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_BaseTimetables_SourceBaseTimetableId",
                        column: x => x.SourceBaseTimetableId,
                        principalTable: "BaseTimetables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BaseTimetableEntryPeriods",
                columns: table => new
                {
                    BaseTimetableEntryId = table.Column<int>(type: "int", nullable: false),
                    BaseTimetablePeriodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseTimetableEntryPeriods", x => new { x.BaseTimetableEntryId, x.BaseTimetablePeriodId });
                    table.ForeignKey(
                        name: "FK_BaseTimetableEntryPeriods_BaseTimetableEntries_BaseTimetableEntryId",
                        column: x => x.BaseTimetableEntryId,
                        principalTable: "BaseTimetableEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaseTimetableEntryPeriods_BaseTimetablePeriods_BaseTimetablePeriodId",
                        column: x => x.BaseTimetablePeriodId,
                        principalTable: "BaseTimetablePeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeeklyScheduleId = table.Column<int>(type: "int", nullable: false),
                    BaseTimetableEntryId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledLessons_BaseTimetableEntries_BaseTimetableEntryId",
                        column: x => x.BaseTimetableEntryId,
                        principalTable: "BaseTimetableEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledLessons_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledLessons_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledLessons_WeeklySchedules_WeeklyScheduleId",
                        column: x => x.WeeklyScheduleId,
                        principalTable: "WeeklySchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledLessonId = table.Column<int>(type: "int", nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActorUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false),
                    PreviousValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleChangeLogs_ScheduledLessons_ScheduledLessonId",
                        column: x => x.ScheduledLessonId,
                        principalTable: "ScheduledLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeeklySchedulePeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeeklyScheduleId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsBreak = table.Column<bool>(type: "bit", nullable: false),
                    CurrentScheduledLessonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklySchedulePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklySchedulePeriods_ScheduledLessons_CurrentScheduledLessonId",
                        column: x => x.CurrentScheduledLessonId,
                        principalTable: "ScheduledLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklySchedulePeriods_WeeklySchedules_WeeklyScheduleId",
                        column: x => x.WeeklyScheduleId,
                        principalTable: "WeeklySchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledLessonPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledLessonId = table.Column<int>(type: "int", nullable: false),
                    WeeklySchedulePeriodId = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledLessonPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledLessonPeriods_ScheduledLessons_ScheduledLessonId",
                        column: x => x.ScheduledLessonId,
                        principalTable: "ScheduledLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledLessonPeriods_WeeklySchedulePeriods_WeeklySchedulePeriodId",
                        column: x => x.WeeklySchedulePeriodId,
                        principalTable: "WeeklySchedulePeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseTimetableEntries_BaseTimetableId",
                table: "BaseTimetableEntries",
                column: "BaseTimetableId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseTimetableEntries_SubjectId",
                table: "BaseTimetableEntries",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseTimetableEntries_TeacherId",
                table: "BaseTimetableEntries",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseTimetableEntryPeriods_BaseTimetablePeriodId",
                table: "BaseTimetableEntryPeriods",
                column: "BaseTimetablePeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseTimetablePeriods_BaseTimetableId_SequenceNumber",
                table: "BaseTimetablePeriods",
                columns: new[] { "BaseTimetableId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseTimetables_ClassId",
                table: "BaseTimetables",
                column: "ClassId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HolidayExceptions_StartDate_EndDate_IsActive",
                table: "HolidayExceptions",
                columns: new[] { "StartDate", "EndDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChangeLogs_ScheduledLessonId_Timestamp",
                table: "ScheduleChangeLogs",
                columns: new[] { "ScheduledLessonId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessonPeriods_ScheduledLessonId",
                table: "ScheduledLessonPeriods",
                column: "ScheduledLessonId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessonPeriods_WeeklySchedulePeriodId",
                table: "ScheduledLessonPeriods",
                column: "WeeklySchedulePeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_BaseTimetableEntryId",
                table: "ScheduledLessons",
                column: "BaseTimetableEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_Date_TeacherId",
                table: "ScheduledLessons",
                columns: new[] { "Date", "TeacherId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_Date_WeeklyScheduleId",
                table: "ScheduledLessons",
                columns: new[] { "Date", "WeeklyScheduleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_SubjectId",
                table: "ScheduledLessons",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_TeacherId",
                table: "ScheduledLessons",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_WeeklyScheduleId",
                table: "ScheduledLessons",
                column: "WeeklyScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedulePeriods_CurrentScheduledLessonId",
                table: "WeeklySchedulePeriods",
                column: "CurrentScheduledLessonId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedulePeriods_WeeklyScheduleId",
                table: "WeeklySchedulePeriods",
                column: "WeeklyScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_AcademicYearId",
                table: "WeeklySchedules",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_ClassId_AcademicYearId_SemesterId_WeekStartDate",
                table: "WeeklySchedules",
                columns: new[] { "ClassId", "AcademicYearId", "SemesterId", "WeekStartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_SemesterId",
                table: "WeeklySchedules",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_SourceBaseTimetableId",
                table: "WeeklySchedules",
                column: "SourceBaseTimetableId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_WeekStartDate",
                table: "WeeklySchedules",
                column: "WeekStartDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseTimetableEntryPeriods");

            migrationBuilder.DropTable(
                name: "HolidayExceptions");

            migrationBuilder.DropTable(
                name: "ScheduleChangeLogs");

            migrationBuilder.DropTable(
                name: "ScheduledLessonPeriods");

            migrationBuilder.DropTable(
                name: "BaseTimetablePeriods");

            migrationBuilder.DropTable(
                name: "WeeklySchedulePeriods");

            migrationBuilder.DropTable(
                name: "ScheduledLessons");

            migrationBuilder.DropTable(
                name: "BaseTimetableEntries");

            migrationBuilder.DropTable(
                name: "WeeklySchedules");

            migrationBuilder.DropTable(
                name: "BaseTimetables");
        }
    }
}
