namespace Shared
{
    public record CreateGradeResultDto
    {
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public int Score { get; set; }
    }
}
