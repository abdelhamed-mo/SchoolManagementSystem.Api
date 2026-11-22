namespace Domain.Entities
{
    public class BaseTimetablePeriod : BaseEntity<int>
    {
        public int BaseTimetableId { get; set; }
        public BaseTimetable BaseTimetable { get; set; } = null!;
        public int SequenceNumber { get; set; }
        public int? PeriodNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsBreak { get; set; }

        public ICollection<BaseTimetableEntryPeriod> Entries { get; set; } = new List<BaseTimetableEntryPeriod>();
    }
}