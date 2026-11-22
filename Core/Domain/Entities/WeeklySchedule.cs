namespace Domain.Entities
{
    public class WeeklySchedule : BaseEntity<int>
    {
        public int ClassId { get; set; }
        public Class Class { get; set; } = null!;

        public int AcademicYearId { get; set; }
        public AcademicYear AcademicYear { get; set; } = null!;

        public int SemesterId { get; set; }
        public Semester Semester { get; set; } = null!;

        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public DateTime GeneratedAt { get; set; }

        public ICollection<WeeklySchedulePeriod> Periods { get; set; } = new List<WeeklySchedulePeriod>();
        public ICollection<ScheduledLesson> Lessons { get; set; } = new List<ScheduledLesson>();
    }
}
