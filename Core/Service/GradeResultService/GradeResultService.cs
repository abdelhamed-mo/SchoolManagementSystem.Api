using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Service.ConcreteSpecifications;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Service.GradeResultService
{
    public class GradeResultService : IGradeResultService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public GradeResultService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<GradeResultDto>> GetGradeResultsAsync(GradeResultsParams parameters)
        {
            var repository = unitOfWork.GetRepository<GradeResult, int>();
            var countSpecification = new GradeResultSpecifications(parameters, applyPagination: false, includeRelated: false);
            var pageSpecification = new GradeResultSpecifications(parameters);

            var total = await repository.CountAsync(countSpecification);
            var results = await repository.GetAllAsync(pageSpecification);
            var dto = mapper.Map<IEnumerable<GradeResultDto>>(results);

            return new PaginatedResultDto<GradeResultDto>(dto.Count(), parameters.PageIndex, total, dto);
        }

        public async Task<GradeResultDto> GetGradeResultByIdAsync(int id)
        {
            var result = await unitOfWork
                .GetRepository<GradeResult, int>()
                .GetAsync(new GradeResultSpecifications(id));

            return result is not null
                ? mapper.Map<GradeResultDto>(result)
                : throw new GradeResultNotFoundException(id);
        }

        public async Task<GradeResultDto> CreateGradeResultAsync(CreateGradeResultDto dto)
        {
            var (student, exam) = await GetAndValidateReferencesAsync(dto.StudentId, dto.ExamId, dto.Score);
            var result = new GradeResult
            {
                StudentId = student.Id,
                ExamId = exam.Id,
                GradeId = exam.GradeId,
                Score = dto.Score
            };

            await unitOfWork.GetRepository<GradeResult, int>().AddAsync(result);
            await unitOfWork.SaveChangesAsync();

            result.Student = student;
            result.Exam = exam;
            result.Grade = exam.Grade;
            return mapper.Map<GradeResultDto>(result);
        }

        public async Task<GradeResultDto> UpdateGradeResultAsync(UpdateGradeResultDto dto)
        {
            var repository = unitOfWork.GetRepository<GradeResult, int>();
            var result = await repository.GetAsync(dto.Id);
            if (result is null)
                throw new GradeResultNotFoundException(dto.Id);

            var (student, exam) = await GetAndValidateReferencesAsync(dto.StudentId, dto.ExamId, dto.Score);

            result.StudentId = student.Id;
            result.ExamId = exam.Id;
            result.GradeId = exam.GradeId;
            result.Score = dto.Score;

            repository.Update(result);
            await unitOfWork.SaveChangesAsync();

            result.Student = student;
            result.Exam = exam;
            result.Grade = exam.Grade;
            return mapper.Map<GradeResultDto>(result);
        }

        public async Task<string> DeleteGradeResultAsync(int id)
        {
            var repository = unitOfWork.GetRepository<GradeResult, int>();
            var result = await repository.GetAsync(id);
            if (result is null)
                throw new GradeResultNotFoundException(id);

            repository.Delete(result);
            await unitOfWork.SaveChangesAsync();

            return $"GradeResult with Id [{id}] was deleted successfully.";
        }

        private async Task<(Student Student, Exam Exam)> GetAndValidateReferencesAsync(
            int studentId,
            int examId,
            int score)
        {
            var student = await unitOfWork.GetRepository<Student, int>().GetAsync(studentId);
            if (student is null)
                throw new StudentNotFoundException(studentId);

            var exam = await unitOfWork
                .GetRepository<Exam, int>()
                .GetAsync(new ExamSpecifications(examId));
            if (exam is null)
                throw new ExamNotFoundException(examId);

            var errors = new List<string>();
            if (student.ClassId != exam.ClassId)
                errors.Add("The selected student does not belong to the class associated with the exam.");
            if (student.GradeId != exam.GradeId)
                errors.Add("The selected student does not belong to the grade associated with the exam.");
            if (score < 0 || score > exam.MaxScore)
                errors.Add($"Score must be between 0 and {exam.MaxScore}.");

            if (errors.Any())
                throw new ValidationException(errors);

            return (student, exam);
        }
    }
}
