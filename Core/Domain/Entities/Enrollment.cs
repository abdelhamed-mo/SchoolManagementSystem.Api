using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
  
    public class Enrollment : BaseEntity<int>
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int ClassId { get; set; }
        public Class Class { get; set; } = null!;

        public int? AcademicYearId { get; set; }
        public AcademicYear? AcademicYear { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
    }

}
