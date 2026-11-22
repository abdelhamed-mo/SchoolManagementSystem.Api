namespace Domain.Entities
{
    public class AcademicYear : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<WeeklySchedule> WeeklySchedules { get; set; } = new List<WeeklySchedule>();
    }
}
