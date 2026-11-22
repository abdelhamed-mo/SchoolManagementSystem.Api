using Domain.Entities;
using Shared.Params;

namespace Service.ConcreteSpecifications
{
    public class GradeResultSpecifications : Specifications<GradeResult>
    {
        public GradeResultSpecifications(
            GradeResultsParams parameters,
            bool applyPagination = true,
            bool includeRelated = true)
            : base(result =>
                (!parameters.StudentId.HasValue || result.StudentId == parameters.StudentId) &&
                (!parameters.ExamId.HasValue || result.ExamId == parameters.ExamId) &&
                (!parameters.GradeId.HasValue || result.GradeId == parameters.GradeId) &&
                (!parameters.ClassId.HasValue || result.Exam.ClassId == parameters.ClassId) &&
                (!parameters.SubjectId.HasValue || result.Exam.SubjectId == parameters.SubjectId) &&
                (!parameters.FromDate.HasValue || result.Exam.ExamDate >= parameters.FromDate) &&
                (!parameters.ToDate.HasValue || result.Exam.ExamDate <= parameters.ToDate))
        {
            if (includeRelated)
            {
                AddInclude(result => result.Exam);
                AddInclude(result => result.Student);
                AddInclude(result => result.Grade);
            }

            if (applyPagination)
                ApplyPagination(parameters.PageSize, parameters.PageIndex);
        }

        public GradeResultSpecifications(int id, bool includeRelated = true)
            : base(result => result.Id == id)
        {
            if (includeRelated)
            {
                AddInclude(result => result.Exam);
                AddInclude(result => result.Student);
                AddInclude(result => result.Grade);
            }
        }
    }
}
