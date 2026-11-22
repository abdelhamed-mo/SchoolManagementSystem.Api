using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public record CreateEnrollmentDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int AcademicYearId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Active";
    }
}
