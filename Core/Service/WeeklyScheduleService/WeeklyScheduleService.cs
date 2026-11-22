using System.Text.Json;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Microsoft.EntityFrameworkCore;
using Service.ConcreteSpecifications;
using Shared;

namespace Service.WeeklyScheduleService
{
    public class WeeklyScheduleService(IUnitOfWork unitOfWork, IMapper mapper) : IWeeklyScheduleService
    {
        public async Task<WeeklyScheduleDto> GenerateAsync(int classId, DateTime weekStartDate)
        {
            var weekStart = SundayOf(weekStartDate);
            var weekEnd = weekStart.AddDays(4);
            var classEntity = await GetClassAsync(classId);
            var academicYear = await ActiveAcademicYearAsync();
            var semester = await ActiveSemesterAsync();
            ValidateContext(academicYear, semester);
            if (weekStart < academicYear.StartDate.Date || weekEnd > academicYear.EndDate.Date || weekStart < semester.StartDate.Date || weekEnd > semester.EndDate.Date)
                throw new ValidationException(new[] { "The requested working week must be within the academic context." });

            var repository = unitOfWork.GetRepository<WeeklySchedule, int>();
            var existing = await repository.GetAsync(new WeeklyScheduleSpecifications(classId, academicYear.Id, semester.Id, weekStart));
            if (existing is not null) return mapper.Map<WeeklyScheduleDto>(existing);
            var timetable = await unitOfWork.GetRepository<BaseTimetable, int>().GetAsync(new BaseTimetableSpecifications(classId))
                ?? throw new BaseTimetableNotFoundException(classId);
            var entryIds = timetable.Entries.Select(entry => entry.Id).ToList();

            var entryPeriodRepository =
                unitOfWork.GetRepository<BaseTimetableEntryPeriod, int>();

            var entryPeriodLinks = entryIds.Count == 0
                ? []
                : (await entryPeriodRepository.GetAllAsync(
                    new BaseTimetableEntryPeriodSpecifications(entryIds))).ToList();

            var periodNumbers = timetable.Periods
                .Where(period => !period.IsBreak)
                .ToDictionary(
                    period => period.Id,
                    period => period.PeriodNumber!.Value);

            var periodsByEntry = entryPeriodLinks
                .GroupBy(link => link.BaseTimetableEntryId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Where(link => periodNumbers.ContainsKey(link.BaseTimetablePeriodId))
                        .Select(link => periodNumbers[link.BaseTimetablePeriodId])
                        .ToList());

            var holidays = (await unitOfWork.GetRepository<HolidayException, int>().GetAllAsync(new HolidayExceptionSpecifications(weekStart, weekEnd))).ToList();
            var schedule = new WeeklySchedule { ClassId = classEntity.Id, AcademicYearId = academicYear.Id, SemesterId = semester.Id, WeekStartDate = weekStart, WeekEndDate = weekEnd, GeneratedAt = DateTime.UtcNow };

            foreach (var day in Enumerable.Range(0, 5))
            {
                var date = weekStart.AddDays(day);
                foreach (var period in timetable.Periods.OrderBy(period => period.SequenceNumber))
                    schedule.Periods.Add(new WeeklySchedulePeriod { WeeklySchedule = schedule, Date = date, SequenceNumber = period.SequenceNumber, PeriodNumber = period.PeriodNumber, StartTime = period.StartTime, EndTime = period.EndTime, IsBreak = period.IsBreak });
            }
            //foreach (var entry in timetable.Entries)
            //{
            //    var date = weekStart.AddDays((int)entry.DayOfWeek);
            //    if (entry.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday || date < academicYear.StartDate.Date || date > academicYear.EndDate.Date || date < semester.StartDate.Date || date > semester.EndDate.Date) continue;
            //    var periods = schedule.Periods.Where(period => period.Date == date && entry.Periods.Any(link => link.BaseTimetablePeriod.PeriodNumber == period.PeriodNumber)).ToList();
            //    if (periods.Count == 0) continue;
            //    var lesson = new ScheduledLesson { WeeklySchedule = schedule, Date = date, StartTime = periods.Min(period => period.StartTime), EndTime = periods.Max(period => period.EndTime), SubjectId = entry.SubjectId, TeacherId = entry.TeacherId, Status = IsHoliday(date, holidays) ? ScheduledLessonStatus.Cancelled : ScheduledLessonStatus.Planned, CreatedAt = DateTime.UtcNow };
            //    foreach (var period in periods)
            //    {
            //        lesson.Periods.Add(new ScheduledLessonPeriod { ScheduledLesson = lesson, WeeklySchedulePeriod = period, PeriodNumber = period.PeriodNumber!.Value, StartTime = period.StartTime, EndTime = period.EndTime });
            //        if (lesson.Status == ScheduledLessonStatus.Planned) period.CurrentScheduledLesson = lesson;
            //    }
            //    schedule.Lessons.Add(lesson);
            //    if (lesson.Status == ScheduledLessonStatus.Cancelled) AddLog(lesson, ScheduleChangeOperationType.Cancelled, ScheduleChangeSource.Holiday, "Lesson cancelled because its date is an active holiday.", "{}", Snapshot(lesson));
            //}
            foreach (var entry in timetable.Entries)
            {
                var date = weekStart.AddDays((int)entry.DayOfWeek);

                if (entry.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday ||
                    date < academicYear.StartDate.Date ||
                    date > academicYear.EndDate.Date ||
                    date < semester.StartDate.Date ||
                    date > semester.EndDate.Date)
                    continue;

                if (!periodsByEntry.TryGetValue(entry.Id, out var entryPeriodNumbers))
                    continue;

                var periods = schedule.Periods
                    .Where(period =>
                        period.Date == date &&
                        period.PeriodNumber.HasValue &&
                        entryPeriodNumbers.Contains(period.PeriodNumber.Value))
                    .ToList();

                if (periods.Count == 0)
                    continue;

                var lesson = new ScheduledLesson
                {
                    WeeklySchedule = schedule,
                    Date = date,
                    StartTime = periods.Min(period => period.StartTime),
                    EndTime = periods.Max(period => period.EndTime),
                    SubjectId = entry.SubjectId,
                    TeacherId = entry.TeacherId,
                    Status = IsHoliday(date, holidays)
                        ? ScheduledLessonStatus.Cancelled
                        : ScheduledLessonStatus.Planned,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var period in periods)
                {
                    lesson.Periods.Add(new ScheduledLessonPeriod
                    {
                        ScheduledLesson = lesson,
                        WeeklySchedulePeriod = period,
                        PeriodNumber = period.PeriodNumber!.Value,
                        StartTime = period.StartTime,
                        EndTime = period.EndTime
                    });

                    if (lesson.Status == ScheduledLessonStatus.Planned)
                        period.CurrentScheduledLesson = lesson;
                }

                schedule.Lessons.Add(lesson);

                if (lesson.Status == ScheduledLessonStatus.Cancelled)
                {
                    AddLog(
                        lesson,
                        ScheduleChangeOperationType.Cancelled,
                        ScheduleChangeSource.Holiday,
                        "Lesson cancelled because its date is an active holiday.",
                        "{}",
                        Snapshot(lesson));
                }
            }
            try
            {
                await unitOfWork.ExecuteInTransactionAsync(async () => { await repository.AddAsync(schedule); await unitOfWork.SaveChangesAsync(); });
            }
            catch (DbUpdateException)
            {
                var concurrent = await repository.GetAsync(new WeeklyScheduleSpecifications(classId, academicYear.Id, semester.Id, weekStart));
                if (concurrent is null) throw;
                return mapper.Map<WeeklyScheduleDto>(concurrent);
            }
            return mapper.Map<WeeklyScheduleDto>(schedule);
        }

        public async Task<WeeklyScheduleDto> GetByClassAndWeekAsync(int classId, DateTime weekStartDate)
        //{
        //    var schedule = await unitOfWork.GetRepository<WeeklySchedule, int>().GetAsync(new WeeklyScheduleSpecifications(classId, SundayOf(weekStartDate)));
        //    return schedule is null ? throw new WeeklyScheduleNotFoundException(classId) : mapper.Map<WeeklyScheduleDto>(schedule);
        //}
        {
            var schedule = await unitOfWork
                .GetRepository<WeeklySchedule, int>()
                .GetAsync(new WeeklyScheduleSpecifications(
                    classId,
                    SundayOf(weekStartDate)));

            if (schedule is null)
                throw new WeeklyScheduleNotFoundException(classId);

            var dto = mapper.Map<WeeklyScheduleDto>(schedule);

            var lessonIds = schedule.Lessons
                .Select(lesson => lesson.Id)
                .ToList();

            if (lessonIds.Count == 0)
                return dto;

            var periodRepository = unitOfWork
                .GetRepository<ScheduledLessonPeriod, int>();

            var lessonPeriods = await periodRepository.GetAllAsync(
                new ScheduledLessonPeriodSpecifications(lessonIds));

            var periodsByLesson = lessonPeriods
                .GroupBy(period => period.ScheduledLessonId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(period => new ScheduledLessonPeriodDto
                    {
                        Id = period.Id,
                        WeeklySchedulePeriodId = period.WeeklySchedulePeriodId,
                        PeriodNumber = period.PeriodNumber,
                        StartTime = period.StartTime,
                        EndTime = period.EndTime
                    }).ToList());

            foreach (var lessonDto in dto.Lessons)
            {
                lessonDto.Periods = periodsByLesson.TryGetValue(
                    lessonDto.Id, out var periods)
                        ? periods
                        : [];
            }
            dto.Periods = dto.Periods
    .OrderBy(period => period.Date)
    .ThenBy(period => period.SequenceNumber)
    .ToList();

            dto.Lessons = dto.Lessons
                .OrderBy(lesson => lesson.Date)
                .ThenBy(lesson => lesson.StartTime)
                .ToList();
            return dto;
        }

        public async Task<IEnumerable<ScheduledLessonDto>> GetLessonsAsync(int weeklyScheduleId)
        {
            var schedule = await GetScheduleAsync(weeklyScheduleId);
            return mapper.Map<IEnumerable<ScheduledLessonDto>>(schedule.Lessons.OrderBy(lesson => lesson.Date).ThenBy(lesson => lesson.StartTime));
        }

        public async Task<ScheduledLessonDto> AddNewLessonAsync(NewScheduledLessonDto dto)
        {
            var schedule = await GetScheduleAsync(dto.WeeklyScheduleId);
            ValidateCurrentWeek(schedule);
            var periods = await ValidatePeriodsAsync(schedule, dto.Date, dto.PeriodNumbers, dto.SubjectId, dto.TeacherId, null, true);
            var lesson = new ScheduledLesson { WeeklySchedule = schedule, WeeklyScheduleId = schedule.Id, Date = dto.Date.Date, StartTime = periods.Min(period => period.StartTime), EndTime = periods.Max(period => period.EndTime), SubjectId = dto.SubjectId, TeacherId = dto.TeacherId, Status = ScheduledLessonStatus.Planned, CreatedAt = DateTime.UtcNow };
            foreach (var period in periods) { lesson.Periods.Add(new ScheduledLessonPeriod { ScheduledLesson = lesson, WeeklySchedulePeriod = period, PeriodNumber = period.PeriodNumber!.Value, StartTime = period.StartTime, EndTime = period.EndTime }); period.CurrentScheduledLesson = lesson; }
            await unitOfWork.ExecuteInTransactionAsync(async () => { await unitOfWork.GetRepository<ScheduledLesson, int>().AddAsync(lesson); AddLog(lesson, ScheduleChangeOperationType.NewLessonAdded, ScheduleChangeSource.Administrator, dto.Reason, "{}", Snapshot(lesson)); await unitOfWork.SaveChangesAsync(); });
            return mapper.Map<ScheduledLessonDto>(lesson);
        }

        public async Task<ScheduledLessonDto> UpdateLessonAsync(UpdateScheduledLessonDto dto)
        {
            var lesson = await unitOfWork.GetRepository<ScheduledLesson, int>().GetAsync(new ScheduledLessonSpecifications(dto.Id)) ?? throw new ScheduledLessonNotFoundException(dto.Id);
            if (lesson.Status == ScheduledLessonStatus.Cancelled) throw new ValidationException(new[] { "A cancelled lesson cannot be restored." });
            var schedule = await GetScheduleAsync(dto.WeeklyScheduleId);
            ValidateCurrentWeek(schedule);
            var periods = await ValidatePeriodsAsync(schedule, dto.Date, dto.PeriodNumbers, dto.SubjectId, dto.TeacherId, dto.Id, true);
            var previous = Snapshot(lesson);
            foreach (var old in lesson.Periods) old.WeeklySchedulePeriod.CurrentScheduledLessonId = null;
            lesson.WeeklyScheduleId = schedule.Id; lesson.WeeklySchedule = schedule; lesson.Date = dto.Date.Date; lesson.StartTime = periods.Min(period => period.StartTime); lesson.EndTime = periods.Max(period => period.EndTime); lesson.SubjectId = dto.SubjectId; lesson.TeacherId = dto.TeacherId; lesson.ModifiedAt = DateTime.UtcNow; lesson.Periods.Clear();
            foreach (var period in periods) { lesson.Periods.Add(new ScheduledLessonPeriod { ScheduledLesson = lesson, WeeklySchedulePeriod = period, PeriodNumber = period.PeriodNumber!.Value, StartTime = period.StartTime, EndTime = period.EndTime }); period.CurrentScheduledLesson = lesson; }
            await unitOfWork.ExecuteInTransactionAsync(async () => { unitOfWork.GetRepository<ScheduledLesson, int>().Update(lesson); AddLog(lesson, ScheduleChangeOperationType.PeriodsChanged, ScheduleChangeSource.Administrator, dto.Reason, previous, Snapshot(lesson)); await unitOfWork.SaveChangesAsync(); });
            return mapper.Map<ScheduledLessonDto>(lesson);
        }

        public async Task<ScheduledLessonDto> CancelLessonAsync(CancelScheduledLessonDto dto)
        {
            var lesson = await unitOfWork.GetRepository<ScheduledLesson, int>().GetAsync(new ScheduledLessonSpecifications(dto.Id)) ?? throw new ScheduledLessonNotFoundException(dto.Id);
            var schedule = await GetScheduleAsync(lesson.WeeklyScheduleId);
            ValidateCurrentWeek(schedule);
            if (lesson.Status == ScheduledLessonStatus.Cancelled) return mapper.Map<ScheduledLessonDto>(lesson);
            var previous = Snapshot(lesson); lesson.Status = ScheduledLessonStatus.Cancelled; lesson.ModifiedAt = DateTime.UtcNow;
            foreach (var period in lesson.Periods) period.WeeklySchedulePeriod.CurrentScheduledLessonId = null;
            await unitOfWork.ExecuteInTransactionAsync(async () => { unitOfWork.GetRepository<ScheduledLesson, int>().Update(lesson); AddLog(lesson, ScheduleChangeOperationType.Cancelled, ScheduleChangeSource.Administrator, dto.Reason, previous, Snapshot(lesson)); await unitOfWork.SaveChangesAsync(); });
            return mapper.Map<ScheduledLessonDto>(lesson);
        }

        public async Task<IEnumerable<ScheduleChangeLogDto>> GetLessonHistoryAsync(int scheduledLessonId)
        {
            _ = await unitOfWork.GetRepository<ScheduledLesson, int>().GetAsync(new ScheduledLessonSpecifications(scheduledLessonId)) ?? throw new ScheduledLessonNotFoundException(scheduledLessonId);
            var logs = await unitOfWork.GetRepository<ScheduleChangeLog, int>().GetAllAsync(new ScheduleChangeLogSpecifications(scheduledLessonId));
            return mapper.Map<IEnumerable<ScheduleChangeLogDto>>(logs.OrderBy(log => log.Timestamp));
        }

        private async Task<List<WeeklySchedulePeriod>> ValidatePeriodsAsync(WeeklySchedule schedule, DateTime date, IEnumerable<int> numbers, int subjectId, int teacherId, int? excludedId, bool requireActive)
        {
            if (requireActive) { var year = await ActiveAcademicYearAsync(); var semester = await ActiveSemesterAsync(); ValidateContext(year, semester); if (schedule.AcademicYearId != year.Id || schedule.SemesterId != semester.Id) throw new ValidationException(new[] { "New scheduling requires the active academic context." }); }
            if (date.Date < schedule.WeekStartDate || date.Date > schedule.WeekEndDate || date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday) throw new ValidationException(new[] { "The lesson date must be a working day in the weekly schedule." });
            var selected = numbers?.Distinct().ToList() ?? []; if (selected.Count == 0) throw new ValidationException(new[] { "At least one period is required." });
            var periods = schedule.Periods.Where(period => period.Date == date.Date && period.PeriodNumber.HasValue && selected.Contains(period.PeriodNumber.Value)).ToList();
            if (periods.Count != selected.Count || periods.Any(period => period.IsBreak || period.CurrentScheduledLessonId.HasValue && period.CurrentScheduledLessonId != excludedId)) throw new ValidationException(new[] { "The selected periods are unavailable or invalid." });
            var classEntity = await GetClassAsync(schedule.ClassId); if (!classEntity.ClassSubjects.Any(link => link.SubjectId == subjectId)) throw new ValidationException(new[] { "The subject is not assigned to the class." });
            var qualified = await unitOfWork.GetRepository<TeacherSubject, int>().GetAllAsync(new TeacherSubjectSpecifications(teacherId)); if (!qualified.Any(link => link.SubjectId == subjectId)) throw new ValidationException(new[] { "The teacher is not qualified for the subject." });
            var lessons = await unitOfWork.GetRepository<ScheduledLesson, int>().GetAllAsync(new ScheduledLessonSpecifications(date.Date, null, teacherId, excludedId));
            if (lessons.Any(lesson => periods.Any(period =>
                 period.StartTime < lesson.EndTime &&
                 lesson.StartTime < period.EndTime))) throw new ValidationException(new[] { "The teacher has an overlapping lesson." });
            return periods;
        }

        private async Task<WeeklySchedule> GetScheduleAsync(int id) => await unitOfWork.GetRepository<WeeklySchedule, int>().GetAsync(new WeeklyScheduleSpecifications(id)) ?? throw new WeeklyScheduleNotFoundException(id);
        private async Task<Class> GetClassAsync(int id) => await unitOfWork.GetRepository<Class, int>().GetAsync(new ClassSpecifications(id)) ?? throw new ClassNotFoundException(id);
        private async Task<AcademicYear> ActiveAcademicYearAsync() => await unitOfWork.GetRepository<AcademicYear, int>().GetAsync(new AcademicYearSpecifications(true)) ?? throw new ValidationException(new[] { "No active academic year exists." });
        private async Task<Semester> ActiveSemesterAsync() => await unitOfWork.GetRepository<Semester, int>().GetAsync(new SemesterSpecifications(true)) ?? throw new ValidationException(new[] { "No active semester exists." });
        private static void ValidateContext(AcademicYear year, Semester semester) { if (semester.AcademicYearId != year.Id) throw new ValidationException(new[] { "The active semester must belong to the active academic year." }); }
        private static DateTime SundayOf(DateTime date) => date.Date.AddDays(-(int)date.DayOfWeek);
        private static bool IsHoliday(DateTime date, IEnumerable<HolidayException> holidays) => holidays.Any(holiday => date.Date >= holiday.StartDate.Date && date.Date <= holiday.EndDate.Date);
        private static string Snapshot(ScheduledLesson lesson) => JsonSerializer.Serialize(new { lesson.WeeklyScheduleId, lesson.Date, lesson.StartTime, lesson.EndTime, lesson.SubjectId, lesson.TeacherId, lesson.Status });
        private static void AddLog(ScheduledLesson lesson, ScheduleChangeOperationType type, ScheduleChangeSource source, string? reason, string previous, string current) => lesson.ChangeLogs.Add(new ScheduleChangeLog { ScheduledLesson = lesson, OperationType = type, Source = source, Reason = reason, PreviousValues = previous, NewValues = current, Timestamp = DateTime.UtcNow });
        private static void ValidateCurrentWeek(WeeklySchedule schedule)
        {
            var currentWeekStart = SundayOf(DateTime.Today);

            if (schedule.WeekStartDate.Date != currentWeekStart.Date)
                throw new ValidationException(
                    new[] { "Only the current week's schedule can be modified." });
        }
    }
}
