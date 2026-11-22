namespace Shared
{
    public record GradeResultDto
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public int PassScore { get; set; }
        public bool IsPassed { get; set; }
    }
}
