using System;

namespace Shared
{
    public record BaseTimetablePeriodDto
    {
        public int Id { get; set; }
        public int SequenceNumber { get; set; }
        public int? PeriodNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsBreak { get; set; }
    }
}