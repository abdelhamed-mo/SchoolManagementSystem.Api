using System;
using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public record BaseTimetableEntryDto
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public IEnumerable<int> PeriodNumbers { get; set; } = [];
    }

    public record CreateBaseTimetableEntryDto
    {
        [Range(1, int.MaxValue)]
        public int SubjectId { get; set; }
        [Range(1, int.MaxValue)]
        public int TeacherId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public IEnumerable<int> PeriodNumbers { get; set; } = [];
    }
}
