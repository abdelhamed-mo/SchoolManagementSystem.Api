using Shared.Params;

namespace Service.ConcreteSpecifications
{
    public class EnrollmentSpecifications : Specifications<Enrollment>
    {
        public EnrollmentSpecifications(EnrollmentsParams parameters, bool applyPagination = true)
            : base(enrollment =>
                (!parameters.StudentId.HasValue || enrollment.StudentId == parameters.StudentId.Value) &&
                (!parameters.ClassId.HasValue || enrollment.ClassId == parameters.ClassId.Value) &&
                (!parameters.AcademicYearId.HasValue || enrollment.AcademicYearId == parameters.AcademicYearId.Value) &&
                (string.IsNullOrWhiteSpace(parameters.Status) || enrollment.Status == parameters.Status) &&
                (!parameters.FromDate.HasValue || enrollment.StartDate >= parameters.FromDate.Value) &&
                (!parameters.ToDate.HasValue || !enrollment.EndDate.HasValue || enrollment.EndDate.Value <= parameters.ToDate.Value))
        {
            AddInclude(enrollment => enrollment.Student);
            AddInclude(enrollment => enrollment.Class);
            AddInclude(enrollment => enrollment.AcademicYear);

            if (applyPagination)
                ApplyPagination(parameters.PageSize, parameters.PageIndex);
        }

        public EnrollmentSpecifications(int id)
            : base(enrollment => enrollment.Id == id)
        {
            AddInclude(enrollment => enrollment.Student);
            AddInclude(enrollment => enrollment.Class);
            AddInclude(enrollment => enrollment.AcademicYear);
        }

        public EnrollmentSpecifications(int studentId, string status)
            : base(enrollment => enrollment.StudentId == studentId && enrollment.Status == status)
        {
            AddInclude(enrollment => enrollment.Student);
            AddInclude(enrollment => enrollment.Class);
            AddInclude(enrollment => enrollment.AcademicYear);
        }

        public EnrollmentSpecifications(int studentId, DateTime startDate, DateTime? endDate, int? excludedId = null)
            : base(enrollment => enrollment.StudentId == studentId &&
                enrollment.Status == "Active" &&
                (!excludedId.HasValue || enrollment.Id != excludedId.Value) &&
                enrollment.StartDate < (endDate ?? DateTime.MaxValue) &&
                (!enrollment.EndDate.HasValue || enrollment.EndDate.Value > startDate))
        {
        }
    }
}
