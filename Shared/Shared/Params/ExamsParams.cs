using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Params
{
    public class ExamsParams
    {
        // filter
        public int? GradeId { get; set; }
        public int? ClassId { get; set; }
        public int? SubjectId { get; set; }
        public int? SemesterId { get; set; }
        public int? AcademicYearId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // pagination
        const int MaxSize = 10;
        const int DefaultSize = 5;
        private int pageSize = DefaultSize;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxSize ? MaxSize : value;
        }

        public int PageIndex { get; set; } = 1;

        // search
        public string? Search { get; set; }
    }
}
