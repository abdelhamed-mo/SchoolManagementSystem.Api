namespace Domain.Entities
{
    public class HolidayException : BaseEntity<int>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RemovedAt { get; set; }
    }
}
