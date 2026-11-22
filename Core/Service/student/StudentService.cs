using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using ServiceAbstraction.student;
using Shared;

namespace Service.StudentService
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        // =========================
        // Students CRUD
        // =========================

        public async Task<IEnumerable<StudentDto>> GetAllAsync()
        {

            var students = await unitOfWork
                .GetRepository<Student, int>()
                .GetAllAsync();

            return students == null
                ? new List<StudentDto>()
                : mapper.Map<IEnumerable<StudentDto>>(students);
        }

        public async Task<StudentDto?> GetByIdAsync(int id)
        {
            var student = await unitOfWork
                .GetRepository<Student, int>()
                .GetAsync(id);

            return student == null
                ? throw new StudentNotFoundException(id)
                : mapper.Map<StudentDto>(student);
        }

        public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
        {
            var classEntity = await unitOfWork.GetRepository<Class, int>().GetAsync(dto.ClassId);
            if (classEntity is null)
                throw new ClassNotFoundException(dto.ClassId);

            var academicYear = await unitOfWork.GetRepository<AcademicYear, int>().GetAsync(dto.AcademicYearId);
            if (academicYear is null)
                throw new AcademicYearNotFoundException(dto.AcademicYearId);
            if (classEntity.AcademicYearId != dto.AcademicYearId)
                throw new ValidationException(new[] { "The selected class must belong to the selected academic year." });
            if (dto.EnrollmentStartDate.Date < academicYear.StartDate.Date || dto.EnrollmentStartDate.Date > academicYear.EndDate.Date)
                throw new ValidationException(new[] { "Enrollment start date must be within the academic year." });

            var student = mapper.Map<Student>(dto);
            student.EnrollmentDate = dto.EnrollmentStartDate.Date;
            student.ClassId = classEntity.Id;
            student.Class = classEntity;

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await unitOfWork.GetRepository<Student, int>().AddAsync(student);
                await unitOfWork.SaveChangesAsync();

                await unitOfWork.GetRepository<Enrollment, int>().AddAsync(new Enrollment
                {
                    Student = student,
                    StudentId = student.Id,
                    Class = classEntity,
                    ClassId = classEntity.Id,
                    AcademicYear = academicYear,
                    AcademicYearId = academicYear.Id,
                    StartDate = dto.EnrollmentStartDate.Date,
                    Status = "Active"
                });
                await unitOfWork.SaveChangesAsync();
            });

            return mapper.Map<StudentDto>(student);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = unitOfWork.GetRepository<Student, int>();
            var student = await repo.GetAsync(id);

            if (student == null)
                return false;

            repo.Delete(student);
            await unitOfWork.SaveChangesAsync();
            return true;
        }

        // =========================
        // Student Business Logic
        // =========================

        // 🟢 الكورسات المتاحة
        public async Task<IEnumerable<StudentCourseDto>> GetAvailableCoursesAsync()
        {
            var subjects = await unitOfWork
                .GetRepository<Subject, int>()
                .GetAllAsync();

            return mapper.Map<IEnumerable<StudentCourseDto>>(subjects);
        }

        // 🟢 الكورسات اللي الطالب مسجل فيها
        public async Task<IEnumerable<ClassDto>> GetClassesAsync(int studentId)
        {
            var classes = await unitOfWork
                .GetRepository<Class, int>()
                .GetAllAsync();

            var studentClasses = classes
                .Where(e => e.Students.Any(s=>s.Id == studentId));
            ////////////////////////////////////////// mapping profile for class needed
            return mapper.Map<IEnumerable<ClassDto>>(studentClasses);
        }
    }
}
