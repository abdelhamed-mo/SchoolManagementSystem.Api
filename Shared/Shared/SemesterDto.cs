namespace Shared
{
    public record SemesterDto
    {
        public int Id { get; set; }
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
