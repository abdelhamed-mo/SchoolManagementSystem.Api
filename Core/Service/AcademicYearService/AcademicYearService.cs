using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Service.ConcreteSpecifications;
using Shared;
using Shared.Params;

namespace Service.AcademicYearService
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public AcademicYearService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<AcademicYearDto>> GetAcademicYearsAsync(AcademicYearsParams parameters)
        {
            var repository = unitOfWork.GetRepository<AcademicYear, int>();
            var countSpecification = new AcademicYearSpecifications(parameters, applyPagination: false, includeSemesters: false);
            var pageSpecification = new AcademicYearSpecifications(parameters);
            var total = await repository.CountAsync(countSpecification);
            var years = await repository.GetAllAsync(pageSpecification);
            var dto = mapper.Map<IEnumerable<AcademicYearDto>>(years);

            return new PaginatedResultDto<AcademicYearDto>(dto.Count(), parameters.PageIndex, total, dto);
        }

        public async Task<AcademicYearDto> GetAcademicYearByIdAsync(int id)
        {
            var year = await GetAcademicYearAsync(id, includeChildren: true);
            return mapper.Map<AcademicYearDto>(year);
        }

        public async Task<AcademicYearDto> GetActiveAcademicYearAsync()
        {
            var year = await unitOfWork.GetRepository<AcademicYear, int>().GetAsync(new AcademicYearSpecifications(true));
            return year is null
                ? throw new ValidationException(new[] { "There is no active academic year." })
                : mapper.Map<AcademicYearDto>(year);
        }

        public async Task<AcademicYearDto> CreateAcademicYearAsync(CreateAcademicYearDto dto)
        {
            ValidateYearValues(dto.Name, dto.StartDate, dto.EndDate);
            await EnsureYearNameAndPeriodAvailableAsync(dto.Name, dto.StartDate, dto.EndDate);

            var year = new AcademicYear
            {
                Name = dto.Name.Trim(),
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                IsActive = false
            };

            await unitOfWork.GetRepository<AcademicYear, int>().AddAsync(year);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<AcademicYearDto>(year);
        }

        public async Task<AcademicYearDto> UpdateAcademicYearAsync(UpdateAcademicYearDto dto)
        {
            var repository = unitOfWork.GetRepository<AcademicYear, int>();
            var year = await GetAcademicYearAsync(dto.Id, includeChildren: true);
            ValidateYearValues(dto.Name, dto.StartDate, dto.EndDate);
            await EnsureYearNameAndPeriodAvailableAsync(dto.Name, dto.StartDate, dto.EndDate, dto.Id);

            if (year.Semesters.Any(semester => semester.StartDate < dto.StartDate.Date || semester.EndDate > dto.EndDate.Date))
                throw new ValidationException(new[] { "The academic year dates must contain all existing semester dates." });

            year.Name = dto.Name.Trim();
            year.StartDate = dto.StartDate.Date;
            year.EndDate = dto.EndDate.Date;
            repository.Update(year);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<AcademicYearDto>(year);
        }

        public async Task<AcademicYearDto> ActivateAcademicYearAsync(int id)
        {
            var repository = unitOfWork.GetRepository<AcademicYear, int>();
            var year = await GetAcademicYearAsync(id, includeChildren: true);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var activeYears = await repository.GetAllAsync(new AcademicYearSpecifications(true));
                foreach (var activeYear in activeYears.Where(activeYear => activeYear.Id != id))
                    activeYear.IsActive = false;

                var activeSemesters = await unitOfWork.GetRepository<Semester, int>().GetAllAsync(new SemesterSpecifications(true));
                foreach (var activeSemester in activeSemesters)
                    activeSemester.IsActive = false;

                year.IsActive = true;
                repository.Update(year);
                await unitOfWork.SaveChangesAsync();
            });

            return mapper.Map<AcademicYearDto>(year);
        }

        public async Task<AcademicYearDto> DeactivateAcademicYearAsync(int id)
        {
            var repository = unitOfWork.GetRepository<AcademicYear, int>();
            var year = await GetAcademicYearAsync(id, includeChildren: true);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                year.IsActive = false;
                foreach (var semester in year.Semesters.Where(semester => semester.IsActive))
                    semester.IsActive = false;

                repository.Update(year);
                await unitOfWork.SaveChangesAsync();
            });

            return mapper.Map<AcademicYearDto>(year);
        }

        public async Task<string> DeleteAcademicYearAsync(int id)
        {
            var repository = unitOfWork.GetRepository<AcademicYear, int>();
            var year = await GetAcademicYearAsync(id, includeChildren: true);
            if (year.Semesters.Any() || year.Classes.Any() || year.Enrollments.Any())
                throw new ValidationException(new[] { "Cannot delete an academic year with historical semesters, classes, or enrollments." });

            repository.Delete(year);
            await unitOfWork.SaveChangesAsync();
            return $"Academic year with Id [{id}] was deleted successfully.";
        }

        private async Task<AcademicYear> GetAcademicYearAsync(int id, bool includeChildren)
        {
            var year = await unitOfWork.GetRepository<AcademicYear, int>().GetAsync(new AcademicYearSpecifications(id, includeChildren));
            return year ?? throw new AcademicYearNotFoundException(id);
        }

        private async Task EnsureYearNameAndPeriodAvailableAsync(string name, DateTime startDate, DateTime endDate, int? excludedId = null)
        {
            var repository = unitOfWork.GetRepository<AcademicYear, int>();
            if (await repository.CountAsync(new AcademicYearSpecifications(name, excludedId)) > 0)
                throw new ValidationException(new[] { "An academic year with the same name already exists." });
            if (await repository.CountAsync(new AcademicYearSpecifications(startDate.Date, endDate.Date, excludedId)) > 0)
                throw new ValidationException(new[] { "Academic year dates cannot overlap another academic year." });
        }

        private static void ValidateYearValues(string name, DateTime startDate, DateTime endDate)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Academic year name is required.");
            if (startDate == default || endDate == default)
                errors.Add("Academic year start and end dates are required.");
            if (startDate.Date >= endDate.Date)
                errors.Add("Academic year start date must be before its end date.");
            if (errors.Any())
                throw new ValidationException(errors);
        }
    }
}
