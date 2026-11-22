using Shared.Params;
using System.Linq.Expressions;

namespace Service.ConcreteSpecifications
{
    public class AttendanceSpecifications : Specifications<Attendance>
    {
        public AttendanceSpecifications(
            AttendancesParams parameters,
            bool applyPagination = true,
            bool includeRelated = true)
            : base(BuildCriteria(parameters))
        {
            if (includeRelated)
            {
                AddInclude(attendance => attendance.Student);
                AddInclude(attendance => attendance.Class);
            }

            if (applyPagination)
                ApplyPagination(parameters.PageSize, parameters.PageIndex);
        }

        public AttendanceSpecifications(int id, bool includeRelated = true)
            : base(attendance => attendance.Id == id)
        {
            if (includeRelated)
            {
                AddInclude(attendance => attendance.Student);
                AddInclude(attendance => attendance.Class);
            }
        }

        public AttendanceSpecifications(int studentId, DateTime date, int? excludedId = null)
            : base(attendance =>
                attendance.StudentId == studentId &&
                attendance.Date >= date.Date &&
                attendance.Date < date.Date.AddDays(1) &&
                (!excludedId.HasValue || attendance.Id != excludedId.Value))
        {
        }

        private static Expression<Func<Attendance, bool>> BuildCriteria(AttendancesParams parameters)
        {
            var fromDate = parameters.FromDate?.Date;
            var toDateExclusive = parameters.ToDate?.Date.AddDays(1);

            return attendance =>
                (!parameters.StudentId.HasValue || attendance.StudentId == parameters.StudentId.Value) &&
                (!parameters.ClassId.HasValue || attendance.ClassId == parameters.ClassId.Value) &&
                (string.IsNullOrWhiteSpace(parameters.Status) || attendance.Status == parameters.Status) &&
                (!fromDate.HasValue || attendance.Date >= fromDate.Value) &&
                (!toDateExclusive.HasValue || attendance.Date < toDateExclusive.Value);
        }
    }
}
