using System.Linq.Expressions;

namespace Service.ConcreteSpecifications
{
    public class ClassSchedulingSpecifications : Specifications<Class>
    {
        public ClassSchedulingSpecifications(int academicYearId)
            : base(classEntity => classEntity.AcademicYearId == academicYearId)
        {
            AddInclude(classEntity => classEntity.ClassSubjects);
        }
    }

    public class BaseTimetableSpecifications : Specifications<BaseTimetable>
    {
        public BaseTimetableSpecifications(int classId)
            : base(timetable => timetable.ClassId == classId)
        {
            AddInclude(timetable => timetable.Entries);
            AddInclude(timetable => timetable.Periods);
        }

        public BaseTimetableSpecifications(int id, bool includeEntries)
            : base(timetable => timetable.Id == id)
        {
            if (includeEntries)
            {
                AddInclude(timetable => timetable.Entries);
                AddInclude(timetable => timetable.Periods);
            }
        }
    }
    public class BaseTimetableEntryPeriodSpecifications : Specifications<BaseTimetableEntryPeriod>
    {
        public BaseTimetableEntryPeriodSpecifications(
            IEnumerable<int> entryIds)
            : base(link => entryIds.Contains(link.BaseTimetableEntryId))
        {
        }
    }
    public class WeeklyScheduleSpecifications : Specifications<WeeklySchedule>
    {
        public WeeklyScheduleSpecifications(int classId, DateTime weekStartDate)
            : base(schedule => schedule.ClassId == classId && schedule.WeekStartDate == weekStartDate)
        {
            AddInclude(schedule => schedule.Lessons);
            AddInclude(schedule => schedule.Periods);
        }

        public WeeklyScheduleSpecifications(int classId, int academicYearId, int semesterId, DateTime weekStartDate)
            : base(schedule => schedule.ClassId == classId &&
                               schedule.AcademicYearId == academicYearId &&
                               schedule.SemesterId == semesterId &&
                               schedule.WeekStartDate == weekStartDate)
        {
            AddInclude(schedule => schedule.Lessons);
            AddInclude(schedule => schedule.Periods);
        }

        public WeeklyScheduleSpecifications(int id)
            : base(schedule => schedule.Id == id)
        {
            AddInclude(schedule => schedule.Lessons);
            AddInclude(schedule => schedule.Periods);
        }

        public WeeklyScheduleSpecifications(DateTime startDate, DateTime endDate)
            : base(schedule => schedule.WeekStartDate <= endDate && schedule.WeekEndDate >= startDate)
        {
            AddInclude(schedule => schedule.Lessons);
            AddInclude(schedule => schedule.Periods);
        }
    }
    public class ScheduledLessonPeriodSpecifications : Specifications<ScheduledLessonPeriod>
    {
        public ScheduledLessonPeriodSpecifications(IEnumerable<int> lessonIds)
            : base(period => lessonIds.Contains(period.ScheduledLessonId))
        {
        }
    }
    public class ScheduledLessonSpecifications : Specifications<ScheduledLesson>
    {
        public ScheduledLessonSpecifications(int id)
            : base(lesson => lesson.Id == id)
        {
            AddInclude(lesson => lesson.WeeklySchedule);
            AddInclude(lesson => lesson.Periods);
        }

        public ScheduledLessonSpecifications(DateTime date, int? classId, int? teacherId, int? excludedId)
            : base(lesson => lesson.Date == date &&
                             (!classId.HasValue || lesson.WeeklySchedule.ClassId == classId.Value) &&
                             lesson.Status == ScheduledLessonStatus.Planned &&
                             (!teacherId.HasValue || lesson.TeacherId == teacherId.Value) &&
                             (!excludedId.HasValue || lesson.Id != excludedId.Value))
        {
            AddInclude(lesson => lesson.Periods);
        }
    }

    public class HolidayExceptionSpecifications : Specifications<HolidayException>
    {
        public HolidayExceptionSpecifications()
            : base(null)
        {
        }

        public HolidayExceptionSpecifications(bool active)
            : base(holiday => holiday.IsActive == active)
        {
        }

        public HolidayExceptionSpecifications(DateTime startDate, DateTime endDate, int? excludedId = null)
            : base(holiday => holiday.IsActive &&
                              holiday.StartDate <= endDate &&
                              holiday.EndDate >= startDate &&
                              (!excludedId.HasValue || holiday.Id != excludedId.Value))
        {
        }
    }

    public class ScheduleChangeLogSpecifications : Specifications<ScheduleChangeLog>
    {
        public ScheduleChangeLogSpecifications(int scheduledLessonId)
            : base(log => log.ScheduledLessonId == scheduledLessonId)
        {
        }
    }
}
