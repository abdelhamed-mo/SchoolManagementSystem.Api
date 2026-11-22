using Shared.Params;

namespace Service.ConcreteSpecifications
{
    public class AcademicYearSpecifications : Specifications<AcademicYear>
    {
        public AcademicYearSpecifications(AcademicYearsParams parameters, bool applyPagination = true, bool includeSemesters = true)
            : base(year =>
                (string.IsNullOrWhiteSpace(parameters.Search) || year.Name.ToUpper().Contains(parameters.Search!.Trim().ToUpper())) &&
                (!parameters.FromDate.HasValue || year.StartDate >= parameters.FromDate.Value) &&
                (!parameters.ToDate.HasValue || year.EndDate <= parameters.ToDate.Value) &&
                (!parameters.IsActive.HasValue || year.IsActive == parameters.IsActive.Value))
        {
            if (includeSemesters)
                AddInclude(year => year.Semesters);

            if (applyPagination)
                ApplyPagination(parameters.PageSize, parameters.PageIndex);
        }

        public AcademicYearSpecifications(int id, bool includeChildren = true)
            : base(year => year.Id == id)
        {
            if (includeChildren)
            {
                AddInclude(year => year.Semesters);
                AddInclude(year => year.Classes);
                AddInclude(year => year.Enrollments);
            }
        }

        public AcademicYearSpecifications(bool active)
            : base(year => year.IsActive == active)
        {
        }

        public AcademicYearSpecifications(string name, int? excludedId = null)
            : base(year => year.Name.ToUpper() == name.Trim().ToUpper() &&
                           (!excludedId.HasValue || year.Id != excludedId.Value))
        {
        }

        public AcademicYearSpecifications(DateTime startDate, DateTime endDate, int? excludedId = null)
            : base(year => year.StartDate < endDate &&
                           year.EndDate > startDate &&
                           (!excludedId.HasValue || year.Id != excludedId.Value))
        {
        }
    }
}
