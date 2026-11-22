using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Service.ConcreteSpecifications;
using Shared.Params;

namespace Service.AttendanceService
{
    public class AttendanceService : IAttendanceService
    {
        private static readonly string[] SupportedStatuses = ["Present", "Absent", "Late"];
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<AttendanceDto>> GetAttendancesAsync(AttendancesParams parameters)
        {
            var repository = unitOfWork.GetRepository<Attendance, int>();
            var countSpecification = new AttendanceSpecifications(parameters, applyPagination: false, includeRelated: false);
            var pageSpecification = new AttendanceSpecifications(parameters);

            var total = await repository.CountAsync(countSpecification);
            var attendances = await repository.GetAllAsync(pageSpecification);
            var dto = mapper.Map<IEnumerable<AttendanceDto>>(attendances);

            return new PaginatedResultDto<AttendanceDto>(dto.Count(), parameters.PageIndex, total, dto);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByStudentAsync(int studentId)
        {
            await GetStudentAsync(studentId);

            var attendances = await unitOfWork
                .GetRepository<Attendance, int>()
                .GetAllAsync(new AttendanceSpecifications(
                    new AttendancesParams { StudentId = studentId },
                    applyPagination: false));

            return mapper.Map<IEnumerable<AttendanceDto>>(attendances);
        }

        public async Task<AttendanceDto> GetAttendanceByIdAsync(int id)
        {
            var attendance = await unitOfWork
                .GetRepository<Attendance, int>()
                .GetAsync(new AttendanceSpecifications(id));

            return attendance is not null
                ? mapper.Map<AttendanceDto>(attendance)
                : throw new AttendanceNotFoundException(id);
        }

        public async Task<AttendanceDto> CreateAttendanceAsync(CreateAttendanceDto dto)
        {
            ValidateAttendanceValues(dto.Date, dto.Status);
            var (student, classEntity) = await GetAndValidateReferencesAsync(dto.StudentId, dto.ClassId);
            await EnsureNotDuplicateAsync(dto.StudentId, dto.Date);

            var attendance = new Attendance
            {
                StudentId = student.Id,
                Student = student,
                ClassId = classEntity.Id,
                Class = classEntity,
                Date = dto.Date,
                Status = dto.Status
            };

            var repository = unitOfWork.GetRepository<Attendance, int>();
            await repository.AddAsync(attendance);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<AttendanceDto>(attendance);
        }

        public async Task<AttendanceDto> UpdateAttendanceAsync(UpdateAttendanceDto dto)
        {
            var repository = unitOfWork.GetRepository<Attendance, int>();
            var attendance = await repository.GetAsync(dto.Id);
            if (attendance is null)
                throw new AttendanceNotFoundException(dto.Id);

            ValidateAttendanceValues(dto.Date, dto.Status);
            var (student, classEntity) = await GetAndValidateReferencesAsync(dto.StudentId, dto.ClassId);
            await EnsureNotDuplicateAsync(dto.StudentId, dto.Date, dto.Id);

            attendance.StudentId = student.Id;
            attendance.Student = student;
            attendance.ClassId = classEntity.Id;
            attendance.Class = classEntity;
            attendance.Date = dto.Date;
            attendance.Status = dto.Status;

            repository.Update(attendance);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<AttendanceDto>(attendance);
        }

        public async Task<string> DeleteAttendanceAsync(int id)
        {
            var repository = unitOfWork.GetRepository<Attendance, int>();
            var attendance = await repository.GetAsync(id);
            if (attendance is null)
                throw new AttendanceNotFoundException(id);

            repository.Delete(attendance);
            await unitOfWork.SaveChangesAsync();

            return $"Attendance with Id [{id}] was deleted successfully.";
        }

        private async Task<(Student Student, Class Class)> GetAndValidateReferencesAsync(int studentId, int classId)
        {
            var student = await GetStudentAsync(studentId);
            var classEntity = await unitOfWork.GetRepository<Class, int>().GetAsync(classId);
            if (classEntity is null)
                throw new ClassNotFoundException(classId);

            if (student.ClassId != classId)
                throw new ValidationException(new[] { "The selected class does not belong to the selected student." });

            return (student, classEntity);
        }

        private async Task<Student> GetStudentAsync(int studentId)
        {
            var student = await unitOfWork.GetRepository<Student, int>().GetAsync(studentId);
            return student ?? throw new StudentNotFoundException(studentId);
        }

        private async Task EnsureNotDuplicateAsync(int studentId, DateTime date, int? excludedId = null)
        {
            var duplicateCount = await unitOfWork
                .GetRepository<Attendance, int>()
                .CountAsync(new AttendanceSpecifications(studentId, date, excludedId));

            if (duplicateCount > 0)
                throw new ValidationException(new[] { "Attendance already exists for this student on the selected date." });
        }

        private static void ValidateAttendanceValues(DateTime date, string status)
        {
            var errors = new List<string>();
            if (date == default)
                errors.Add("Attendance date is required.");
            if (!SupportedStatuses.Contains(status, StringComparer.Ordinal))
                errors.Add("Attendance status must be Present, Absent, or Late.");

            if (errors.Any())
                throw new ValidationException(errors);
        }
    }
}
