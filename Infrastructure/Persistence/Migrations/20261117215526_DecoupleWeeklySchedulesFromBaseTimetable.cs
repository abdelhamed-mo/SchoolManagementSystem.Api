using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleWeeklySchedulesFromBaseTimetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledLessons_BaseTimetableEntries_BaseTimetableEntryId",
                table: "ScheduledLessons");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklySchedules_BaseTimetables_SourceBaseTimetableId",
                table: "WeeklySchedules");

            migrationBuilder.DropIndex(
                name: "IX_WeeklySchedules_SourceBaseTimetableId",
                table: "WeeklySchedules");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledLessons_BaseTimetableEntryId",
                table: "ScheduledLessons");

            migrationBuilder.DropColumn(
                name: "SourceBaseTimetableId",
                table: "WeeklySchedules");

            migrationBuilder.DropColumn(
                name: "BaseTimetableEntryId",
                table: "ScheduledLessons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceBaseTimetableId",
                table: "WeeklySchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BaseTimetableEntryId",
                table: "ScheduledLessons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_SourceBaseTimetableId",
                table: "WeeklySchedules",
                column: "SourceBaseTimetableId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_BaseTimetableEntryId",
                table: "ScheduledLessons",
                column: "BaseTimetableEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledLessons_BaseTimetableEntries_BaseTimetableEntryId",
                table: "ScheduledLessons",
                column: "BaseTimetableEntryId",
                principalTable: "BaseTimetableEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklySchedules_BaseTimetables_SourceBaseTimetableId",
                table: "WeeklySchedules",
                column: "SourceBaseTimetableId",
                principalTable: "BaseTimetables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
