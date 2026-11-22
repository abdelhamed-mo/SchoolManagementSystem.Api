using System;
using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public record ScheduledLessonDto
    {
        public int Id { get; set; }
        public int WeeklyScheduleId { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public ScheduledLessonStatusDto Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public IEnumerable<ScheduledLessonPeriodDto> Periods { get; set; } = [];
    }

    public enum ScheduledLessonStatusDto
    {
        Planned = 1,
        Cancelled = 2
    }

    public record UpdateScheduledLessonDto
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
        [Range(1, int.MaxValue)]
        public int WeeklyScheduleId { get; set; }
        public DateTime Date { get; set; }
        [Range(1, int.MaxValue)]
        public int SubjectId { get; set; }
        [Range(1, int.MaxValue)]
        public int TeacherId { get; set; }
        public IEnumerable<int> PeriodNumbers { get; set; } = [];
        public string? Reason { get; set; }
    }

    public record NewScheduledLessonDto
    {
        [Range(1, int.MaxValue)]
        public int WeeklyScheduleId { get; set; }
        public DateTime Date { get; set; }
        [Range(1, int.MaxValue)]
        public int SubjectId { get; set; }
        [Range(1, int.MaxValue)]
        public int TeacherId { get; set; }
        public IEnumerable<int> PeriodNumbers { get; set; } = [];
        public string? Reason { get; set; }
    }

    public record CancelScheduledLessonDto
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
        public string? Reason { get; set; }
    }

    public record ScheduledLessonPeriodDto
    {
        public int Id { get; set; }
        public int WeeklySchedulePeriodId { get; set; }
        public int PeriodNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
