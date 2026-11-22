namespace Domain.Entities
{
    public enum ScheduledLessonStatus
    {
        Planned = 1,
        Cancelled = 2
    }

    public class ScheduledLesson : BaseEntity<int>
    {
        public int WeeklyScheduleId { get; set; }
        public WeeklySchedule WeeklySchedule { get; set; } = null!;

        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;
        public ScheduledLessonStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public ICollection<ScheduledLessonPeriod> Periods { get; set; } = new List<ScheduledLessonPeriod>();
        public ICollection<ScheduleChangeLog> ChangeLogs { get; set; } = new List<ScheduleChangeLog>();
    }
}
