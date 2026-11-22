using System;

namespace Shared
{
    public record ScheduleChangeLogDto
    {
        public int Id { get; set; }
        public int ScheduledLessonId { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Reason { get; set; }
        public string? ActorUserId { get; set; }
        public string Source { get; set; } = string.Empty;
        public string PreviousValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
    }
}
