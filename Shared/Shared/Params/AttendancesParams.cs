using System;

namespace Shared.Params
{
    public class AttendancesParams
    {
        public int? StudentId { get; set; }
        public int? ClassId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        const int MaxSize = 10;
        const int DefaultSize = 5;
        private int pageSize = DefaultSize;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxSize ? MaxSize : value;
        }

        public int PageIndex { get; set; } = 1;
    }
}
