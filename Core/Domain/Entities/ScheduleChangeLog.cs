namespace Domain.Entities
{
    public enum ScheduleChangeOperationType
    {
        Generated = 1,
        TeacherChanged = 2,
        TimeChanged = 3,
        SubjectChanged = 4,
        ClassChanged = 5,
        NewLessonAdded = 6,
        PeriodsChanged = 7,
        Cancelled = 8
    }

    public enum ScheduleChangeSource
    {
        Generation = 1,
        Administrator = 2,
        Holiday = 3
    }

    public class ScheduleChangeLog : BaseEntity<int>
    {
        public int ScheduledLessonId { get; set; }
        public ScheduledLesson ScheduledLesson { get; set; } = null!;

        public ScheduleChangeOperationType OperationType { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Reason { get; set; }
        public string? ActorUserId { get; set; }
        public ScheduleChangeSource Source { get; set; }
        public string PreviousValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
    }
}
