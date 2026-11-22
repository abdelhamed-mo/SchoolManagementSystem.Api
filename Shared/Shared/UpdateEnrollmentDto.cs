using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public record UpdateEnrollmentDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
