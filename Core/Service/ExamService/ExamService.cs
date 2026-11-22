using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Service.ConcreteSpecifications;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Service.ExamService
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ExamService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<ExamDto>> GetExamsAsync(ExamsParams _params)
        {
            var repository = unitOfWork.GetRepository<Exam, int>();
            var countSpecification = new ExamSpecifications(_params, applyPagination: false, includeRelated: false);
            var pageSpecification = new ExamSpecifications(_params);

            var total = await repository.CountAsync(countSpecification);
            var exams = await repository.GetAllAsync(pageSpecification);
            var dto = mapper.Map<IEnumerable<ExamDto>>(exams);

            return new PaginatedResultDto<ExamDto>(dto.Count(), _params.PageIndex, total, dto);
        }

        public async Task<ExamDto> GetExamByIdAsync(int id)
        {
            var exam = await unitOfWork
                .GetRepository<Exam, int>()
                .GetAsync(new ExamSpecifications(id));

            return exam is not null
                ? mapper.Map<ExamDto>(exam)
                : throw new ExamNotFoundException(id);
        }

        public async Task<ExamDto> CreateExamAsync(CreateExamDto dto)
        {
            ValidateExamValues(dto.Name, dto.MaxScore, dto.PassScore);
            await ValidateReferencesAsync(dto.SubjectId, dto.ClassId, dto.GradeId, dto.SemesterId);

            var exam = new Exam
            {
                Name = dto.Name,
                ExamDate = dto.ExamDate,
                MaxScore = dto.MaxScore,
                PassScore = dto.PassScore,
                SubjectId = dto.SubjectId,
                ClassId = dto.ClassId,
                GradeId = dto.GradeId,
                SemesterId = dto.SemesterId
            };

            var repository = unitOfWork.GetRepository<Exam, int>();
            await repository.AddAsync(exam);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<ExamDto>(exam);
        }

        public async Task<ExamDto> UpdateExamAsync(UpdateExamDto dto)
        {
            var repository = unitOfWork.GetRepository<Exam, int>();
            var exam = await repository.GetAsync(dto.Id);
            if (exam is null)
                throw new ExamNotFoundException(dto.Id);

            ValidateExamValues(dto.Name, dto.MaxScore, dto.PassScore);
            await ValidateReferencesAsync(dto.SubjectId, dto.ClassId, dto.GradeId, dto.SemesterId);

            exam.Name = dto.Name;
            exam.ExamDate = dto.ExamDate;
            exam.MaxScore = dto.MaxScore;
            exam.PassScore = dto.PassScore;
            exam.SubjectId = dto.SubjectId;
            exam.ClassId = dto.ClassId;
            exam.GradeId = dto.GradeId;
            exam.SemesterId = dto.SemesterId;

            repository.Update(exam);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<ExamDto>(exam);
        }

        public async Task<string> DeleteExamAsync(int id)
        {
            var repository = unitOfWork.GetRepository<Exam, int>();
            var exam = await repository.GetAsync(new ExamSpecifications(id, includeGradeResults: true));
            if (exam is null)
                throw new ExamNotFoundException(id);

            if (exam.GradeResults is not null && exam.GradeResults.Any())
                throw new ValidationException(new[] { "Cannot delete an exam with existing grade results." });

            repository.Delete(exam);
            await unitOfWork.SaveChangesAsync();

            return $"Exam with Id [{id}] was deleted successfully.";
        }

        private async Task ValidateReferencesAsync(int subjectId, int classId, int gradeId, int semesterId)
        {
            var subject = await unitOfWork.GetRepository<Subject, int>().GetAsync(subjectId);
            if (subject is null)
                throw new SubjectNotFoundException(subjectId);

            var classEntity = await unitOfWork
                .GetRepository<Class, int>()
                .GetAsync(new ClassSpecifications(classId));
            if (classEntity is null)
                throw new ClassNotFoundException(classId);

            var grade = await unitOfWork.GetRepository<Grade, int>().GetAsync(gradeId);
            if (grade is null)
                throw new GradeNotFoundException(gradeId);

            var semester = await unitOfWork.GetRepository<Semester, int>().GetAsync(new SemesterSpecifications(semesterId));
            if (semester is null)
                throw new SemesterNotFoundException(semesterId);

            var errors = new List<string>();
            if (classEntity.GradeId != gradeId)
                errors.Add("The selected class does not belong to the selected grade.");

            if (classEntity.AcademicYearId != semester.AcademicYearId)
                errors.Add("The selected class and semester must belong to the same academic year.");

            if (classEntity.ClassSubjects is null || !classEntity.ClassSubjects.Any(s => s.SubjectId == subjectId))
                errors.Add("The selected subject is not assigned to the selected class.");

            if (errors.Any())
                throw new ValidationException(errors);
        }

        private static void ValidateExamValues(string name, int maxScore, int passScore)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Exam name is required.");
            if (maxScore <= 0)
                errors.Add("MaxScore must be positive.");
            if (passScore < 0)
                errors.Add("PassScore cannot be negative.");
            if (passScore > maxScore)
                errors.Add("PassScore cannot exceed MaxScore.");

            if (errors.Any())
                throw new ValidationException(errors);
        }
    }
}
