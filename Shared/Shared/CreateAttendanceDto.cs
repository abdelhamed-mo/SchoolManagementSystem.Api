using System;
using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public record CreateAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public int ClassId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
