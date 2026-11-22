using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Service.ConcreteSpecifications;
using Shared;
using Shared.Params;

namespace Service.SemesterService
{
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public SemesterService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<SemesterDto>> GetSemestersAsync(SemestersParams parameters)
        {
            var repository = unitOfWork.GetRepository<Semester, int>();
            var countSpecification = new SemesterSpecifications(parameters, applyPagination: false, includeAcademicYear: false);
            var pageSpecification = new SemesterSpecifications(parameters);
            var total = await repository.CountAsync(countSpecification);
            var semesters = await repository.GetAllAsync(pageSpecification);
            var dto = mapper.Map<IEnumerable<SemesterDto>>(semesters);

            return new PaginatedResultDto<SemesterDto>(dto.Count(), parameters.PageIndex, total, dto);
        }

        public async Task<PaginatedResultDto<SemesterDto>> GetSemestersByAcademicYearAsync(int academicYearId, SemestersParams parameters)
        {
            await EnsureAcademicYearExistsAsync(academicYearId);
            parameters.AcademicYearId = academicYearId;
            return await GetSemestersAsync(parameters);
        }

        public async Task<SemesterDto> GetSemesterByIdAsync(int id)
        {
            var semester = await GetSemesterAsync(id);
            return mapper.Map<SemesterDto>(semester);
        }

        public async Task<SemesterDto> GetActiveSemesterAsync()
        {
            var semester = await unitOfWork.GetRepository<Semester, int>().GetAsync(new SemesterSpecifications(true));
            return semester is null
                ? throw new ValidationException(new[] { "There is no active semester." })
                : mapper.Map<SemesterDto>(semester);
        }

        public async Task<SemesterDto> CreateSemesterAsync(CreateSemesterDto dto)
        {
            var year = await EnsureAcademicYearExistsAsync(dto.AcademicYearId);
            ValidateSemesterValues(dto.Name, dto.StartDate, dto.EndDate, year);
            await EnsureSemesterNameAndPeriodAvailableAsync(dto.AcademicYearId, dto.Name, dto.StartDate, dto.EndDate);

            var semester = new Semester
            {
                AcademicYearId = dto.AcademicYearId,
                AcademicYear = year,
                Name = dto.Name.Trim(),
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                DisplayOrder = dto.DisplayOrder,
                IsActive = false
            };

            await unitOfWork.GetRepository<Semester, int>().AddAsync(semester);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<SemesterDto>(semester);
        }

        public async Task<SemesterDto> UpdateSemesterAsync(UpdateSemesterDto dto)
        {
            var repository = unitOfWork.GetRepository<Semester, int>();
            var semester = await GetSemesterAsync(dto.Id);
            var year = await EnsureAcademicYearExistsAsync(dto.AcademicYearId);
            ValidateSemesterValues(dto.Name, dto.StartDate, dto.EndDate, year);
            await EnsureSemesterNameAndPeriodAvailableAsync(dto.AcademicYearId, dto.Name, dto.StartDate, dto.EndDate, dto.Id);

            if (semester.Exams.Any() && semester.AcademicYearId != dto.AcademicYearId)
                throw new ValidationException(new[] { "A semester with exams cannot be moved to another academic year." });

            semester.AcademicYearId = dto.AcademicYearId;
            semester.AcademicYear = year;
            semester.Name = dto.Name.Trim();
            semester.StartDate = dto.StartDate.Date;
            semester.EndDate = dto.EndDate.Date;
            semester.DisplayOrder = dto.DisplayOrder;
            repository.Update(semester);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<SemesterDto>(semester);
        }

        public async Task<SemesterDto> ActivateSemesterAsync(int id)
        {
            var repository = unitOfWork.GetRepository<Semester, int>();
            var semester = await GetSemesterAsync(id);
            var year = await EnsureAcademicYearExistsAsync(semester.AcademicYearId);
            if (!year.IsActive)
                throw new ValidationException(new[] { "Only a semester belonging to the active academic year can be activated." });

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var activeSemesters = await repository.GetAllAsync(new SemesterSpecifications(true));
                foreach (var activeSemester in activeSemesters.Where(activeSemester => activeSemester.Id != id))
                    activeSemester.IsActive = false;

                semester.IsActive = true;
                repository.Update(semester);
                await unitOfWork.SaveChangesAsync();
            });

            return mapper.Map<SemesterDto>(semester);
        }

        public async Task<SemesterDto> DeactivateSemesterAsync(int id)
        {
            var repository = unitOfWork.GetRepository<Semester, int>();
            var semester = await GetSemesterAsync(id);
            semester.IsActive = false;
            repository.Update(semester);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<SemesterDto>(semester);
        }

        public async Task<string> DeleteSemesterAsync(int id)
        {
            var repository = unitOfWork.GetRepository<Semester, int>();
            var semester = await GetSemesterAsync(id);
            if (semester.Exams.Any())
                throw new ValidationException(new[] { "Cannot delete a semester with historical exams." });

            repository.Delete(semester);
            await unitOfWork.SaveChangesAsync();
            return $"Semester with Id [{id}] was deleted successfully.";
        }

        private async Task<Semester> GetSemesterAsync(int id)
        {
            var semester = await unitOfWork.GetRepository<Semester, int>().GetAsync(new SemesterSpecifications(id));
            return semester ?? throw new SemesterNotFoundException(id);
        }

        private async Task<AcademicYear> EnsureAcademicYearExistsAsync(int id)
        {
            var year = await unitOfWork.GetRepository<AcademicYear, int>().GetAsync(id);
            return year ?? throw new AcademicYearNotFoundException(id);
        }

        private async Task EnsureSemesterNameAndPeriodAvailableAsync(int academicYearId, string name, DateTime startDate, DateTime endDate, int? excludedId = null)
        {
            var repository = unitOfWork.GetRepository<Semester, int>();
            if (await repository.CountAsync(new SemesterSpecifications(academicYearId, name, excludedId)) > 0)
                throw new ValidationException(new[] { "A semester with the same name already exists in this academic year." });
            if (await repository.CountAsync(new SemesterSpecifications(academicYearId, startDate.Date, endDate.Date, excludedId)) > 0)
                throw new ValidationException(new[] { "Semester dates cannot overlap another semester in this academic year." });
        }

        private static void ValidateSemesterValues(string name, DateTime startDate, DateTime endDate, AcademicYear year)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Semester name is required.");
            if (startDate == default || endDate == default)
                errors.Add("Semester start and end dates are required.");
            if (startDate.Date >= endDate.Date)
                errors.Add("Semester start date must be before its end date.");
            if (startDate.Date < year.StartDate.Date || endDate.Date > year.EndDate.Date)
                errors.Add("Semester dates must be within the parent academic year.");
            if (errors.Any())
                throw new ValidationException(errors);
        }
    }
}
