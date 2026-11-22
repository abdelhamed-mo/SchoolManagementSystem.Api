using Shared.Params;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.ConcreteSpecifications
{
    public class ExamSpecifications : Specifications<Exam>
    {
        public ExamSpecifications(ExamsParams _params, bool applyPagination = true, bool includeRelated = true)
            : base(e =>
                (string.IsNullOrWhiteSpace(_params.Search) || e.Name.ToUpper().Contains(_params.Search!.ToUpper().Trim())) &&
                (!_params.GradeId.HasValue || e.GradeId == _params.GradeId) &&
                (!_params.ClassId.HasValue || e.ClassId == _params.ClassId) &&
                (!_params.SubjectId.HasValue || e.SubjectId == _params.SubjectId) &&
                (!_params.SemesterId.HasValue || e.SemesterId == _params.SemesterId) &&
                (!_params.AcademicYearId.HasValue || (e.Semester != null && e.Semester.AcademicYearId == _params.AcademicYearId)) &&
                (!_params.FromDate.HasValue || e.ExamDate >= _params.FromDate) &&
                (!_params.ToDate.HasValue || e.ExamDate <= _params.ToDate))
        {
            if (includeRelated)
            {
                AddInclude(e => e.Subject);
                AddInclude(e => e.Class);
                AddInclude(e => e.Grade);
                AddInclude("Semester.AcademicYear");
            }

            if (applyPagination)
                ApplyPagination(_params.PageSize, _params.PageIndex);
        }

        public ExamSpecifications(int id, bool includeGradeResults = false)
            : base(e => e.Id == id)
        {
            AddInclude(e => e.Subject);
            AddInclude(e => e.Class);
            AddInclude(e => e.Grade);
            AddInclude("Semester.AcademicYear");

            if (includeGradeResults)
                AddInclude(e => e.GradeResults);
        }
    }
}
