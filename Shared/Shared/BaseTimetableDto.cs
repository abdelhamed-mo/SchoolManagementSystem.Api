using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public record BaseTimetableDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public TimeOnly SchoolDayStartTime { get; set; }
        public int? BreakAfterPeriodNumber { get; set; }
        public int? BreakDurationMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public IEnumerable<BaseTimetablePeriodDto> Periods { get; set; } = [];
        public IEnumerable<BaseTimetableEntryDto> Entries { get; set; } = [];
    }

    public record CreateBaseTimetableDto
    {
        [Range(1, int.MaxValue)]
        public int ClassId { get; set; }
        public TimeOnly SchoolDayStartTime { get; set; }
        public IEnumerable<int> PeriodDurationsMinutes { get; set; } = [];
        public int? BreakAfterPeriodNumber { get; set; }
        public int? BreakDurationMinutes { get; set; }
        public IEnumerable<CreateBaseTimetableEntryDto> Entries { get; set; } = [];
    }

    public record UpdateBaseTimetableDto : CreateBaseTimetableDto
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
    }
}
