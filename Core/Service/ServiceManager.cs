using Domain.Contracts;
using ServiceAbstraction;
using ServiceAbstraction.Admin;
using ServiceAbstraction.Parent;
using ServiceAbstraction.student;
using Service.GradeService;
using Service.ExamService;
using Service.WeeklyScheduleService;
using Microsoft.Extensions.Logging;
using GenerationService = Service.WeeklyScheduleService.WeeklyScheduleGenerationService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service
{
    public class ServiceManger : IServiceManager
    {
        readonly Lazy<IAdminService> adminService;
        readonly Lazy<IStudentService> studentService;
        readonly Lazy<IAuthenticationService> authenticationService;
        readonly Lazy<IParentService> parentService;
        readonly Lazy<IGradeService> gradeService;
        readonly Lazy<IExamService> examService;
        readonly Lazy<IGradeResultService> gradeResultService;
        readonly Lazy<IAttendanceService> attendanceService;
        readonly Lazy<IAcademicYearService> academicYearService;
        readonly Lazy<ISemesterService> semesterService;
        readonly Lazy<IEnrollmentService> enrollmentService;
        readonly Lazy<IBaseTimetableService> baseTimetableService;
        readonly Lazy<IWeeklyScheduleService> weeklyScheduleService;
        readonly Lazy<IWeeklyScheduleGenerationService> weeklyScheduleGenerationService;
        readonly Lazy<IHolidayService> holidayService;

        public ServiceManger(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ILogger<GenerationService> generationLogger)
        {
            adminService = new Lazy<IAdminService>(() => new AdminService.AdminService(unitOfWork, mapper, userManager));
            studentService = new Lazy<IStudentService>(() => new StudentService.StudentService(unitOfWork, mapper));
            authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(userManager, unitOfWork, mapper, configuration));
            parentService = new Lazy<IParentService>(() => new ParentService.ParentService(unitOfWork, mapper, userManager));
            gradeService = new Lazy<IGradeService>(() => new Service.GradeService.GradeService(unitOfWork, mapper));
            examService = new Lazy<IExamService>(() => new Service.ExamService.ExamService(unitOfWork, mapper));
            gradeResultService = new Lazy<IGradeResultService>(() => new Service.GradeResultService.GradeResultService(unitOfWork, mapper));
            attendanceService = new Lazy<IAttendanceService>(() => new Service.AttendanceService.AttendanceService(unitOfWork, mapper));
            academicYearService = new Lazy<IAcademicYearService>(() => new Service.AcademicYearService.AcademicYearService(unitOfWork, mapper));
            semesterService = new Lazy<ISemesterService>(() => new Service.SemesterService.SemesterService(unitOfWork, mapper));
            enrollmentService = new Lazy<IEnrollmentService>(() => new Service.EnrollmentService.EnrollmentService(unitOfWork, mapper));

            baseTimetableService = new Lazy<IBaseTimetableService>(() => new BaseTimetableService.BaseTimetableService(unitOfWork, mapper));

            weeklyScheduleService = new Lazy<IWeeklyScheduleService>(() => new WeeklyScheduleService.WeeklyScheduleService(unitOfWork, mapper));

            weeklyScheduleGenerationService = new Lazy<IWeeklyScheduleGenerationService>(() => new GenerationService(unitOfWork, weeklyScheduleService.Value, generationLogger));

            holidayService = new Lazy<IHolidayService>(() => new HolidayService.HolidayService(unitOfWork, mapper));
        }

        public IAdminService AdminService => adminService.Value;
        public IAuthenticationService AuthenticationService => authenticationService.Value;
        public IStudentService StudentService => studentService.Value;
        public IParentService ParentService => parentService.Value;
        public IGradeService GradeService => gradeService.Value;
        public IExamService ExamService => examService.Value;
        public IGradeResultService GradeResultService => gradeResultService.Value;
        public IAttendanceService AttendanceService => attendanceService.Value;
        public IAcademicYearService AcademicYearService => academicYearService.Value;
        public ISemesterService SemesterService => semesterService.Value;
        public IEnrollmentService EnrollmentService => enrollmentService.Value;
        public IBaseTimetableService BaseTimetableService => baseTimetableService.Value;
        public IWeeklyScheduleService WeeklyScheduleService => weeklyScheduleService.Value;
        public IWeeklyScheduleGenerationService WeeklyScheduleGenerationService => weeklyScheduleGenerationService.Value;
        public IHolidayService HolidayService => holidayService.Value;
    }
}
