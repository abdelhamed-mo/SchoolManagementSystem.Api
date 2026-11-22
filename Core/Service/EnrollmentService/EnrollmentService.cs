using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Service.ConcreteSpecifications;
using Shared;
using Shared.Params;

namespace Service.EnrollmentService
{
    public class EnrollmentService : IEnrollmentService
    {
        private static readonly string[] SupportedStatuses = ["Active", "Completed", "Transferred", "Withdrawn"];
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public EnrollmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<EnrollmentDto>> GetEnrollmentsAsync(EnrollmentsParams parameters)
        {
            var repository = unitOfWork.GetRepository<Enrollment, int>();
            var countSpecification = new EnrollmentSpecifications(parameters, applyPagination: false);
            var pageSpecification = new EnrollmentSpecifications(parameters);
            var total = await repository.CountAsync(countSpecification);
            var enrollments = await repository.GetAllAsync(pageSpecification);
            var dto = mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);

            return new PaginatedResultDto<EnrollmentDto>(dto.Count(), parameters.PageIndex, total, dto);
        }

        public async Task<EnrollmentDto> GetEnrollmentByIdAsync(int id)
        {
            var enrollment = await GetEnrollmentAsync(id);
            return mapper.Map<EnrollmentDto>(enrollment);
        }

        public async Task<IEnumerable<EnrollmentDto>> GetStudentEnrollmentHistoryAsync(int studentId)
        {
            await EnsureStudentExistsAsync(studentId);
            var enrollments = await unitOfWork.GetRepository<Enrollment, int>().GetAllAsync(
                new EnrollmentSpecifications(new EnrollmentsParams { StudentId = studentId }, applyPagination: false));
            return mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
        }

        public async Task<EnrollmentDto> GetCurrentStudentEnrollmentAsync(int studentId)
        {
            await EnsureStudentExistsAsync(studentId);
            var enrollment = await unitOfWork.GetRepository<Enrollment, int>().GetAsync(
                new EnrollmentSpecifications(studentId, "Active"));
            return enrollment is null
                ? throw new ValidationException(new[] { "The student has no active enrollment." })
                : mapper.Map<EnrollmentDto>(enrollment);
        }

        public async Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto dto)
        {
            var (student, classEntity, year) = await ValidateReferencesAsync(dto.StudentId, dto.ClassId, dto.AcademicYearId);
            ValidateEnrollmentValues(dto.StartDate, dto.EndDate, dto.Status, year);
            await EnsureNoConflictingActiveEnrollmentAsync(dto.StudentId, dto.StartDate, dto.EndDate);

            if (dto.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                student.ClassId = classEntity.Id;

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                Student = student,
                ClassId = classEntity.Id,
                Class = classEntity,
                AcademicYearId = year.Id,
                AcademicYear = year,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate?.Date,
                Status = NormalizeStatus(dto.Status)
            };

            await unitOfWork.GetRepository<Enrollment, int>().AddAsync(enrollment);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<EnrollmentDto>(enrollment);
        }

        public async Task<EnrollmentDto> TransferStudentAsync(TransferEnrollmentDto dto)
        {
            var (student, classEntity, year) = await ValidateReferencesAsync(dto.StudentId, dto.ClassId, dto.AcademicYearId);
            ValidateEnrollmentValues(dto.StartDate, null, "Active", year);

            Enrollment? previousEnrollment = null;
            Enrollment? newEnrollment = null;
            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                previousEnrollment = await unitOfWork.GetRepository<Enrollment, int>().GetAsync(
                    new EnrollmentSpecifications(dto.StudentId, "Active"));
                if (previousEnrollment is not null && previousEnrollment.ClassId == classEntity.Id)
                    throw new ValidationException(new[] { "The student is already actively enrolled in the selected class." });

                await EnsureNoConflictingActiveEnrollmentAsync(dto.StudentId, dto.StartDate, null, previousEnrollment?.Id);

                if (previousEnrollment is not null)
                {
                    if (!previousEnrollment.StartDate.HasValue)
                        throw new ValidationException(new[] { "The previous enrollment has no start date and cannot be transferred automatically." });

                    if (dto.StartDate.Date < previousEnrollment.StartDate.Value.Date)
                        throw new ValidationException(new[] { "The transfer date cannot be before the previous enrollment start date." });

                    previousEnrollment.EndDate = dto.StartDate.Date;
                    previousEnrollment.Status = "Transferred";
                    unitOfWork.GetRepository<Enrollment, int>().Update(previousEnrollment);
                }

                newEnrollment = new Enrollment
                {
                    StudentId = student.Id,
                    Student = student,
                    ClassId = classEntity.Id,
                    Class = classEntity,
                    AcademicYearId = year.Id,
                    AcademicYear = year,
                    StartDate = dto.StartDate.Date,
                    Status = "Active"
                };
                await unitOfWork.GetRepository<Enrollment, int>().AddAsync(newEnrollment);
                student.ClassId = classEntity.Id;
                unitOfWork.GetRepository<Student, int>().Update(student);
                await unitOfWork.SaveChangesAsync();
            });

            return mapper.Map<EnrollmentDto>(newEnrollment);
        }

        public async Task<EnrollmentDto> UpdateEnrollmentAsync(UpdateEnrollmentDto dto)
        {
            var repository = unitOfWork.GetRepository<Enrollment, int>();
            var enrollment = await GetEnrollmentAsync(dto.Id);
            var classEntity = await unitOfWork.GetRepository<Class, int>().GetAsync(dto.ClassId);
            if (classEntity is null)
                throw new ClassNotFoundException(dto.ClassId);
            if (classEntity.AcademicYearId != enrollment.AcademicYearId)
                throw new ValidationException(new[] { "The selected class must belong to the enrollment academic year." });
            if (enrollment.Status == "Active" && enrollment.ClassId != dto.ClassId)
                throw new ValidationException(new[] { "Use the transfer operation to change an active student's class." });

            if (enrollment.AcademicYear is null)
                throw new ValidationException(new[] { "The enrollment has no academic year and cannot be updated through this operation." });

            ValidateEnrollmentValues(dto.StartDate, dto.EndDate, dto.Status, enrollment.AcademicYear);
            await EnsureNoConflictingActiveEnrollmentAsync(enrollment.StudentId, dto.StartDate, dto.EndDate, dto.Id);

            enrollment.ClassId = dto.ClassId;
            enrollment.Class = classEntity;
            enrollment.StartDate = dto.StartDate.Date;
            enrollment.EndDate = dto.EndDate?.Date;
            enrollment.Status = NormalizeStatus(dto.Status);
            if (enrollment.Status == "Active")
                enrollment.Student.ClassId = classEntity.Id;

            repository.Update(enrollment);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<EnrollmentDto>(enrollment);
        }

        public async Task<string> DeleteEnrollmentAsync(int id)
        {
            await GetEnrollmentAsync(id);
            throw new ValidationException(new[] { "Historical enrollment records cannot be deleted." });
        }

        private async Task<Enrollment> GetEnrollmentAsync(int id)
        {
            var enrollment = await unitOfWork.GetRepository<Enrollment, int>().GetAsync(new EnrollmentSpecifications(id));
            return enrollment ?? throw new ValidationException(new[] { $"Enrollment with Id [{id}] is not found." });
        }

        private async Task<Student> EnsureStudentExistsAsync(int id)
        {
            var student = await unitOfWork.GetRepository<Student, int>().GetAsync(id);
            return student ?? throw new StudentNotFoundException(id);
        }

        private async Task<(Student Student, Class Class, AcademicYear AcademicYear)> ValidateReferencesAsync(int studentId, int classId, int academicYearId)
        {
            var student = await EnsureStudentExistsAsync(studentId);
            var classEntity = await unitOfWork.GetRepository<Class, int>().GetAsync(classId);
            if (classEntity is null)
                throw new ClassNotFoundException(classId);
            var year = await unitOfWork.GetRepository<AcademicYear, int>().GetAsync(academicYearId);
            if (year is null)
                throw new AcademicYearNotFoundException(academicYearId);
            if (classEntity.AcademicYearId != academicYearId)
                throw new ValidationException(new[] { "The selected class must belong to the selected academic year." });
            return (student, classEntity, year);
        }

        private async Task EnsureNoConflictingActiveEnrollmentAsync(int studentId, DateTime startDate, DateTime? endDate, int? excludedId = null)
        {
            var count = await unitOfWork.GetRepository<Enrollment, int>().CountAsync(
                new EnrollmentSpecifications(studentId, startDate.Date, endDate?.Date, excludedId));
            if (count > 0)
                throw new ValidationException(new[] { "The student has a conflicting active enrollment period." });
        }

        private static void ValidateEnrollmentValues(DateTime startDate, DateTime? endDate, string status, AcademicYear year)
        {
            var errors = new List<string>();
            if (startDate == default)
                errors.Add("Enrollment start date is required.");
            if (endDate.HasValue && startDate.Date > endDate.Value.Date)
                errors.Add("Enrollment start date must not be after its end date.");
            if (startDate.Date < year.StartDate.Date || (endDate.HasValue && endDate.Value.Date > year.EndDate.Date))
                errors.Add("Enrollment dates must be within the academic year.");
            if (!SupportedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                errors.Add("Enrollment status is not supported.");
            if (status.Equals("Active", StringComparison.OrdinalIgnoreCase) && endDate.HasValue)
                errors.Add("An active enrollment must not have an end date.");
            if (errors.Any())
                throw new ValidationException(errors);
        }

        private static string NormalizeStatus(string status)
            => SupportedStatuses.First(value => value.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
