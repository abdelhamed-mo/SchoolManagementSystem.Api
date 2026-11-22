namespace Domain.Entities
{
    public class ScheduledLessonPeriod : BaseEntity<int>
    {
        public int ScheduledLessonId { get; set; }
        public ScheduledLesson ScheduledLesson { get; set; } = null!;
        public int WeeklySchedulePeriodId { get; set; }
        public WeeklySchedulePeriod WeeklySchedulePeriod { get; set; } = null!;
        public int PeriodNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}