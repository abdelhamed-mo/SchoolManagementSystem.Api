namespace Domain.Entities
{
    public class Semester : BaseEntity<int>
    {
        public int AcademicYearId { get; set; }
        public AcademicYear AcademicYear { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public ICollection<WeeklySchedule> WeeklySchedules { get; set; } = new List<WeeklySchedule>();
    }
}
