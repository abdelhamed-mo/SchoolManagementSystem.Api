namespace Domain.Entities
{
    public class BaseTimetable : BaseEntity<int>
    {
        public int ClassId { get; set; }
        public Class Class { get; set; } = null!;
        public TimeOnly SchoolDayStartTime { get; set; }
        public int? BreakAfterPeriodNumber { get; set; }
        public int? BreakDurationMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public ICollection<BaseTimetablePeriod> Periods { get; set; } = new List<BaseTimetablePeriod>();
        public ICollection<BaseTimetableEntry> Entries { get; set; } = new List<BaseTimetableEntry>();
    }
}
