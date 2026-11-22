using ServiceAbstraction.Admin;
using ServiceAbstraction.Parent;
using ServiceAbstraction.student;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceAbstraction
{
	public interface IServiceManager
	{
		public IAdminService AdminService { get; }
		public IStudentService StudentService { get; }
		public IAuthenticationService AuthenticationService { get; }
		public IParentService ParentService { get; }
		public IGradeService GradeService { get; }
		public IExamService ExamService { get; }
		public IGradeResultService GradeResultService { get; }
		public IAttendanceService AttendanceService { get; }
		public IAcademicYearService AcademicYearService { get; }
		public ISemesterService SemesterService { get; }
		public IEnrollmentService EnrollmentService { get; }
		public IBaseTimetableService BaseTimetableService { get; }
		public IWeeklyScheduleService WeeklyScheduleService { get; }
		public IWeeklyScheduleGenerationService WeeklyScheduleGenerationService { get; }
		public IHolidayService HolidayService { get; }
	}
}
