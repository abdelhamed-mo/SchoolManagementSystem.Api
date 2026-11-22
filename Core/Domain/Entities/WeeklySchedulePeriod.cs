namespace Domain.Entities
{
    public class WeeklySchedulePeriod : BaseEntity<int>
    {
        public int WeeklyScheduleId { get; set; }
        public WeeklySchedule WeeklySchedule { get; set; } = null!;
        public DateTime Date { get; set; }
        public int SequenceNumber { get; set; }
        public int? PeriodNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsBreak { get; set; }
        public int? CurrentScheduledLessonId { get; set; }
        public ScheduledLesson? CurrentScheduledLesson { get; set; }

        public ICollection<ScheduledLessonPeriod> LessonPeriods { get; set; } = new List<ScheduledLessonPeriod>();
    }
}