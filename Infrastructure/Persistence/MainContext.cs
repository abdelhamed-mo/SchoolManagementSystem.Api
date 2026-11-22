using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Migrations;

namespace Persistence
{
	public class MainContext(DbContextOptions<MainContext> options)
		: IdentityDbContext(options)
	{
		// DbSets

		public DbSet<Student> Students => Set<Student>();
		public DbSet<Teacher> Teachers => Set<Teacher>();
		public DbSet<Parent> Parents => Set<Parent>();
		public DbSet<Admin> Admins => Set<Admin>();

		public DbSet<Class> Classes => Set<Class>();
		public DbSet<Subject> Subjects => Set<Subject>();
		//public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
		//public DbSet<ClassTeacher> TeacherClasses => Set<ClassTeacher>();
		public DbSet<Grade> Grades => Set<Grade>();
		public DbSet<Attendance> Attendances => Set<Attendance>();
		public DbSet<Exam> Exams => Set<Exam>();
		public DbSet<GradeResult> GradeResults => Set<GradeResult>();
		public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
		public DbSet<Semester> Semesters => Set<Semester>();
		public DbSet<Enrollment> Enrollments => Set<Enrollment>();
		public DbSet<BaseTimetable> BaseTimetables => Set<BaseTimetable>();
		public DbSet<BaseTimetablePeriod> BaseTimetablePeriods => Set<BaseTimetablePeriod>();
		public DbSet<BaseTimetableEntry> BaseTimetableEntries => Set<BaseTimetableEntry>();
		public DbSet<BaseTimetableEntryPeriod> BaseTimetableEntryPeriods => Set<BaseTimetableEntryPeriod>();
		public DbSet<WeeklySchedule> WeeklySchedules => Set<WeeklySchedule>();
		public DbSet<WeeklySchedulePeriod> WeeklySchedulePeriods => Set<WeeklySchedulePeriod>();
		public DbSet<ScheduledLesson> ScheduledLessons => Set<ScheduledLesson>();
		public DbSet<ScheduledLessonPeriod> ScheduledLessonPeriods => Set<ScheduledLessonPeriod>();
		public DbSet<HolidayException> HolidayExceptions => Set<HolidayException>();
		public DbSet<ScheduleChangeLog> ScheduleChangeLogs => Set<ScheduleChangeLog>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
		
			base.OnModelCreating(modelBuilder);

			
			modelBuilder.Entity<Student>()
				.HasOne(s => s.User)
				.WithMany()
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Class>()
				.Property(c => c.LegacyAcademicYear)
				.HasColumnName("AcademicYear")
				.IsRequired();

			modelBuilder.Entity<AcademicYear>()
				.HasMany(year => year.Semesters)
				.WithOne(semester => semester.AcademicYear)
				.HasForeignKey(semester => semester.AcademicYearId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<AcademicYear>()
				.HasMany(year => year.Classes)
				.WithOne(classEntity => classEntity.AcademicYear)
				.HasForeignKey(classEntity => classEntity.AcademicYearId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<AcademicYear>()
				.HasMany(year => year.Enrollments)
				.WithOne(enrollment => enrollment.AcademicYear)
				.HasForeignKey(enrollment => enrollment.AcademicYearId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Semester>()
				.HasMany(semester => semester.Exams)
				.WithOne(exam => exam.Semester)
				.HasForeignKey(exam => exam.SemesterId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Student>()
				.HasMany(student => student.Enrollments)
				.WithOne(enrollment => enrollment.Student)
				.HasForeignKey(enrollment => enrollment.StudentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Class>()
				.HasMany(classEntity => classEntity.Enrollments)
				.WithOne(enrollment => enrollment.Class)
				.HasForeignKey(enrollment => enrollment.ClassId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<AcademicYear>()
				.HasIndex(year => year.Name)
				.IsUnique();

			modelBuilder.Entity<Semester>()
				.HasIndex(semester => new { semester.AcademicYearId, semester.Name })
				.IsUnique();

			modelBuilder.Entity<Semester>()
				.HasIndex(semester => semester.AcademicYearId);

			modelBuilder.Entity<Class>()
				.HasIndex(classEntity => classEntity.AcademicYearId);

			modelBuilder.Entity<Enrollment>()
				.HasIndex(enrollment => new { enrollment.StudentId, enrollment.AcademicYearId });

			modelBuilder.Entity<Enrollment>()
				.HasIndex(enrollment => enrollment.ClassId);

			modelBuilder.Entity<Exam>()
				.HasIndex(exam => exam.SemesterId);

			modelBuilder.Entity<Attendance>()
				.HasOne(attendance => attendance.Class)
				.WithMany(classEntity => classEntity.Attendances)
				.HasForeignKey(attendance => attendance.ClassId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Attendance>()
				.HasOne(attendance => attendance.Student)
				.WithMany(student => student.Attendances)
				.HasForeignKey(attendance => attendance.StudentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Exam>()
				.HasOne(exam => exam.Class)
				.WithMany(classEntity => classEntity.Exams)
				.HasForeignKey(exam => exam.ClassId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Exam>()
				.HasOne(exam => exam.Subject)
				.WithMany(subject => subject.Exams)
				.HasForeignKey(exam => exam.SubjectId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<GradeResult>()
				.HasOne(result => result.Exam)
				.WithMany(exam => exam.GradeResults)
				.HasForeignKey(result => result.ExamId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<GradeResult>()
				.HasOne(result => result.Student)
				.WithMany(student => student.GradeResults)
				.HasForeignKey(result => result.StudentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Teacher>()
				.HasOne(s => s.User)
				.WithMany()
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Parent>()
				.HasOne(s => s.User)
				.WithMany()
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Admin>()
				.HasOne(s => s.User)
				.WithMany()
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Grade>()
				.HasMany(g => g.Students)
				.WithOne(s=> s.Grade)
				.HasForeignKey(s => s.GradeId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Grade>()
				.HasMany(g => g.Exams)
				.WithOne(s=> s.Grade)
				.HasForeignKey(s => s.GradeId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Grade>()
				.HasMany(g => g.GradeResults)
				.WithOne(s=> s.Grade)
				.HasForeignKey(s => s.GradeId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Grade>()
				.HasMany(g => g.Classes)
				.WithOne(s=> s.Grade)
				.HasForeignKey(s => s.GradeId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Student>()
				.HasOne(s => s.Class)
				.WithMany(c => c.Students)
				.HasForeignKey(s => s.ClassId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ClassTeacher>()
				.HasKey(tc => new { tc.TeacherId, tc.ClassId });

			modelBuilder.Entity<ClassTeacher>()
				.HasOne(tc => tc.Teacher)
				.WithMany(t => t.TeacherClasses)
				.HasForeignKey(tc => tc.TeacherId);

			modelBuilder.Entity<ClassTeacher>()
				.HasOne(tc => tc.Class)
				.WithMany(c => c.ClassTeachers)
				.HasForeignKey(tc => tc.ClassId);

			modelBuilder.Entity<ClassSubject>()
				.HasKey(tc => new { tc.SubjectId, tc.ClassId });

			modelBuilder.Entity<ClassSubject>()
				.HasOne(tc => tc.Subject)
				.WithMany(t => t.ClassSubjects)
				.HasForeignKey(tc => tc.SubjectId);

			modelBuilder.Entity<ClassSubject>()
				.HasOne(tc => tc.Class)
				.WithMany(c => c.ClassSubjects)
				.HasForeignKey(tc => tc.ClassId);

			modelBuilder.Entity<TeacherSubject>()
				.HasKey(tc => new { tc.SubjectId, tc.TeacherId });

			modelBuilder.Entity<TeacherSubject>()
				.HasOne(tc => tc.Subject)
				.WithMany(t => t.TeacherSubjects)
				.HasForeignKey(tc => tc.SubjectId);

			modelBuilder.Entity<TeacherSubject>()
				.HasOne(tc => tc.Teacher)
				.WithMany(c => c.TeacherSubjects)
				.HasForeignKey(tc => tc.TeacherId);

			modelBuilder.Entity<BaseTimetable>()
				.HasOne(timetable => timetable.Class)
				.WithMany(classEntity => classEntity.BaseTimetables)
				.HasForeignKey(timetable => timetable.ClassId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<BaseTimetable>()
				.HasIndex(timetable => timetable.ClassId)
				.IsUnique();

			modelBuilder.Entity<BaseTimetable>()
				.Property(timetable => timetable.SchoolDayStartTime)
				.HasColumnType("time");

			modelBuilder.Entity<BaseTimetableEntry>()
				.HasOne(entry => entry.BaseTimetable)
				.WithMany(timetable => timetable.Entries)
				.HasForeignKey(entry => entry.BaseTimetableId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<BaseTimetableEntry>()
				.HasOne(entry => entry.Subject)
				.WithMany(subject => subject.BaseTimetableEntries)
				.HasForeignKey(entry => entry.SubjectId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<BaseTimetableEntry>()
				.HasOne(entry => entry.Teacher)
				.WithMany(teacher => teacher.BaseTimetableEntries)
				.HasForeignKey(entry => entry.TeacherId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<BaseTimetableEntry>()
				.Property(entry => entry.DayOfWeek)
				.HasConversion<int>();

			modelBuilder.Entity<BaseTimetableEntry>()
				.HasKey(entry => entry.Id);

			modelBuilder.Entity<BaseTimetablePeriod>()
				.HasOne(period => period.BaseTimetable)
				.WithMany(timetable => timetable.Periods)
				.HasForeignKey(period => period.BaseTimetableId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<BaseTimetablePeriod>()
				.Property(period => period.StartTime)
				.HasColumnType("time");

			modelBuilder.Entity<BaseTimetablePeriod>()
				.Property(period => period.EndTime)
				.HasColumnType("time");

			modelBuilder.Entity<BaseTimetablePeriod>()
				.HasIndex(period => new { period.BaseTimetableId, period.SequenceNumber })
				.IsUnique();

			modelBuilder.Entity<BaseTimetableEntryPeriod>()
				.HasKey(link => new { link.BaseTimetableEntryId, link.BaseTimetablePeriodId });

			modelBuilder.Entity<BaseTimetableEntryPeriod>()
				.HasOne(link => link.BaseTimetableEntry)
				.WithMany(entry => entry.Periods)
				.HasForeignKey(link => link.BaseTimetableEntryId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<BaseTimetableEntryPeriod>()
				.HasOne(link => link.BaseTimetablePeriod)
				.WithMany(period => period.Entries)
				.HasForeignKey(link => link.BaseTimetablePeriodId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<WeeklySchedule>()
				.HasOne(schedule => schedule.Class)
				.WithMany(classEntity => classEntity.WeeklySchedules)
				.HasForeignKey(schedule => schedule.ClassId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<WeeklySchedule>()
				.HasOne(schedule => schedule.AcademicYear)
				.WithMany(year => year.WeeklySchedules)
				.HasForeignKey(schedule => schedule.AcademicYearId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<WeeklySchedule>()
				.HasOne(schedule => schedule.Semester)
				.WithMany(semester => semester.WeeklySchedules)
				.HasForeignKey(schedule => schedule.SemesterId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<WeeklySchedule>()
				.HasIndex(schedule => new { schedule.ClassId, schedule.AcademicYearId, schedule.SemesterId, schedule.WeekStartDate })
				.IsUnique();

			modelBuilder.Entity<WeeklySchedule>()
				.HasIndex(schedule => schedule.WeekStartDate);

			modelBuilder.Entity<WeeklySchedulePeriod>()
				.HasOne(period => period.WeeklySchedule)
				.WithMany(schedule => schedule.Periods)
				.HasForeignKey(period => period.WeeklyScheduleId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<WeeklySchedulePeriod>()
				.HasOne(period => period.CurrentScheduledLesson)
				.WithMany()
				.HasForeignKey(period => period.CurrentScheduledLessonId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<WeeklySchedulePeriod>()
				.Property(period => period.StartTime)
				.HasColumnType("time");

			modelBuilder.Entity<WeeklySchedulePeriod>()
				.Property(period => period.EndTime)
				.HasColumnType("time");

			modelBuilder.Entity<ScheduledLesson>()
				.HasOne(lesson => lesson.WeeklySchedule)
				.WithMany(schedule => schedule.Lessons)
				.HasForeignKey(lesson => lesson.WeeklyScheduleId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ScheduledLesson>()
				.HasOne(lesson => lesson.Subject)
				.WithMany(subject => subject.ScheduledLessons)
				.HasForeignKey(lesson => lesson.SubjectId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ScheduledLesson>()
				.HasOne(lesson => lesson.Teacher)
				.WithMany(teacher => teacher.ScheduledLessons)
				.HasForeignKey(lesson => lesson.TeacherId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ScheduledLesson>()
				.Property(lesson => lesson.Status)
				.HasConversion<int>();

			modelBuilder.Entity<ScheduledLesson>()
				.HasIndex(lesson => new { lesson.Date, lesson.TeacherId });

			modelBuilder.Entity<ScheduledLesson>()
				.HasIndex(lesson => new { lesson.Date, lesson.WeeklyScheduleId });

			modelBuilder.Entity<ScheduledLessonPeriod>()
				.HasOne(period => period.ScheduledLesson)
				.WithMany(lesson => lesson.Periods)
				.HasForeignKey(period => period.ScheduledLessonId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ScheduledLessonPeriod>()
				.HasOne(period => period.WeeklySchedulePeriod)
				.WithMany(schedulePeriod => schedulePeriod.LessonPeriods)
				.HasForeignKey(period => period.WeeklySchedulePeriodId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ScheduledLessonPeriod>()
				.Property(period => period.StartTime)
				.HasColumnType("time");

			modelBuilder.Entity<ScheduledLessonPeriod>()
				.Property(period => period.EndTime)
				.HasColumnType("time");

			modelBuilder.Entity<HolidayException>()
				.HasIndex(holiday => new { holiday.StartDate, holiday.EndDate, holiday.IsActive });

			modelBuilder.Entity<ScheduleChangeLog>()
				.HasOne(log => log.ScheduledLesson)
				.WithMany(lesson => lesson.ChangeLogs)
				.HasForeignKey(log => log.ScheduledLessonId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ScheduleChangeLog>()
				.Property(log => log.OperationType)
				.HasConversion<int>();

			modelBuilder.Entity<ScheduleChangeLog>()
				.Property(log => log.Source)
				.HasConversion<int>();

			modelBuilder.Entity<ScheduleChangeLog>()
				.HasIndex(log => new { log.ScheduledLessonId, log.Timestamp });

		}
	}
}
