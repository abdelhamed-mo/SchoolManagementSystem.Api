using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public record UpdateSemesterDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int AcademicYearId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DisplayOrder { get; set; }
    }
}
