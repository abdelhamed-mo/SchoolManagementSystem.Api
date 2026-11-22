using Shared.Params;

namespace Service.ConcreteSpecifications
{
    public class SemesterSpecifications : Specifications<Semester>
    {
        public SemesterSpecifications(SemestersParams parameters, bool applyPagination = true, bool includeAcademicYear = true)
            : base(semester =>
                (!parameters.AcademicYearId.HasValue || semester.AcademicYearId == parameters.AcademicYearId.Value) &&
                (string.IsNullOrWhiteSpace(parameters.Search) || semester.Name.ToUpper().Contains(parameters.Search!.Trim().ToUpper())) &&
                (!parameters.FromDate.HasValue || semester.StartDate >= parameters.FromDate.Value) &&
                (!parameters.ToDate.HasValue || semester.EndDate <= parameters.ToDate.Value) &&
                (!parameters.IsActive.HasValue || semester.IsActive == parameters.IsActive.Value))
        {
            if (includeAcademicYear)
                AddInclude(semester => semester.AcademicYear);

            if (applyPagination)
                ApplyPagination(parameters.PageSize, parameters.PageIndex);
        }

        public SemesterSpecifications(int id, bool includeAcademicYear = true)
            : base(semester => semester.Id == id)
        {
            if (includeAcademicYear)
                AddInclude(semester => semester.AcademicYear);

            AddInclude(semester => semester.Exams);
        }

        public SemesterSpecifications(int academicYearId, string name, int? excludedId = null)
            : base(semester => semester.AcademicYearId == academicYearId &&
                               semester.Name.ToUpper() == name.Trim().ToUpper() &&
                               (!excludedId.HasValue || semester.Id != excludedId.Value))
        {
        }

        public SemesterSpecifications(bool active)
            : base(semester => semester.IsActive == active)
        {
        }

        public SemesterSpecifications(int academicYearId, DateTime startDate, DateTime endDate, int? excludedId = null)
            : base(semester => semester.AcademicYearId == academicYearId &&
                               semester.StartDate < endDate &&
                               semester.EndDate > startDate &&
                               (!excludedId.HasValue || semester.Id != excludedId.Value))
        {
        }
    }
}
