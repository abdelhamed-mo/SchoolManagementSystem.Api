namespace Shared.Params
{
    public class SemestersParams
    {
        public int? AcademicYearId { get; set; }
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsActive { get; set; }

        private const int MaxSize = 50;
        private const int DefaultSize = 10;
        private int pageSize = DefaultSize;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxSize ? MaxSize : value < 1 ? DefaultSize : value;
        }

        public int PageIndex { get; set; } = 1;
    }
}
