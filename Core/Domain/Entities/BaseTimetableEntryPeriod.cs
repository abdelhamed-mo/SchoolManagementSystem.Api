namespace Domain.Entities
{
    public class BaseTimetableEntryPeriod
    {
        public int BaseTimetableEntryId { get; set; }
        public BaseTimetableEntry BaseTimetableEntry { get; set; } = null!;
        public int BaseTimetablePeriodId { get; set; }
        public BaseTimetablePeriod BaseTimetablePeriod { get; set; } = null!;
    }
}