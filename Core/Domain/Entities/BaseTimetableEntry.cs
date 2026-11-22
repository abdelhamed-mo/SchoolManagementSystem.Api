namespace Domain.Entities
{
    public class BaseTimetableEntry : BaseEntity<int>
    {
        public int BaseTimetableId { get; set; }
        public BaseTimetable BaseTimetable { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        public ICollection<BaseTimetableEntryPeriod> Periods { get; set; } = new List<BaseTimetableEntryPeriod>();
    }
}
