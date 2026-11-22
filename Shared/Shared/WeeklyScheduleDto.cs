using System;
using System.Collections.Generic;

namespace Shared
{
    public record WeeklyScheduleDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int AcademicYearId { get; set; }
        public int SemesterId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public IEnumerable<WeeklySchedulePeriodDto> Periods { get; set; } = [];
        public IEnumerable<ScheduledLessonDto> Lessons { get; set; } = [];
    }

    public record WeeklySchedulePeriodDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SequenceNumber { get; set; }
        public int? PeriodNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsBreak { get; set; }
        public int? CurrentScheduledLessonId { get; set; }
    }
}
