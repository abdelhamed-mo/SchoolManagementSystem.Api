using System;
using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public record WeeklyScheduleParams
    {
        [Range(1, int.MaxValue)]
        public int ClassId { get; set; }
        public DateTime WeekStartDate { get; set; }
    }
}
