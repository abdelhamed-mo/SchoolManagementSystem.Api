using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared
{
    public record CreateExamDto
    {
        [Required]
        public string Name { get; set; }
        public DateTime ExamDate { get; set; }
        public int MaxScore { get; set; }
        public int PassScore { get; set; }
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public int GradeId { get; set; }
        public int SemesterId { get; set; }
    }
}
