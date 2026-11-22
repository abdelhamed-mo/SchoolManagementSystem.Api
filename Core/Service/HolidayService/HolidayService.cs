using System.Text.Json;
using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Service.ConcreteSpecifications;
using Shared;

namespace Service.HolidayService
{
    public class HolidayService(IUnitOfWork unitOfWork, IMapper mapper) : IHolidayService
    {
        public async Task<HolidayExceptionDto> CreateAsync(CreateHolidayExceptionDto dto)
        {
            var startDate = dto.StartDate.Date;
            var endDate = dto.EndDate.Date;
            if (startDate > endDate)
                throw new ValidationException(new[] { "Holiday start date cannot be after its end date." });

            var holidayRepository = unitOfWork.GetRepository<HolidayException, int>();
            if (await holidayRepository.GetAsync(new HolidayExceptionSpecifications(startDate, endDate)) is not null)
                throw new ValidationException(new[] { "The selected holiday range overlaps an active holiday." });

            var holiday = new HolidayException
            {
                StartDate = startDate,
                EndDate = endDate,
                Reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (await holidayRepository.GetAsync(new HolidayExceptionSpecifications(startDate, endDate)) is not null)
                    throw new ValidationException(new[] { "The selected holiday range overlaps an active holiday." });

                await holidayRepository.AddAsync(holiday);
                var schedules = await unitOfWork.GetRepository<WeeklySchedule, int>()
                    .GetAllAsync(new WeeklyScheduleSpecifications(startDate, endDate));
                foreach (var schedule in schedules)
                {
                    foreach (var lesson in schedule.Lessons.Where(lesson =>
                                 lesson.Status == ScheduledLessonStatus.Planned &&
                                 lesson.Date.Date >= startDate && lesson.Date.Date <= endDate))
                    {
                        var previousValues = Snapshot(lesson);
                        lesson.Status = ScheduledLessonStatus.Cancelled;
                        lesson.ModifiedAt = DateTime.UtcNow;
                        await unitOfWork.GetRepository<ScheduleChangeLog, int>().AddAsync(CreateChangeLog(
                            lesson,
                            ScheduleChangeOperationType.Cancelled,
                            ScheduleChangeSource.Holiday,
                            holiday.Reason ?? "Lesson cancelled because its date is an active holiday.",
                            previousValues,
                            Snapshot(lesson)));
                    }
                }

                await unitOfWork.SaveChangesAsync();
            });

            return mapper.Map<HolidayExceptionDto>(holiday);
        }

        public async Task<IEnumerable<HolidayExceptionDto>> GetAllAsync()
        {
            var holidays = await unitOfWork.GetRepository<HolidayException, int>()
                .GetAllAsync(new HolidayExceptionSpecifications());
            return mapper.Map<IEnumerable<HolidayExceptionDto>>(holidays.OrderByDescending(holiday => holiday.StartDate));
        }

        public async Task<string> RemoveAsync(int id)
        {
            var repository = unitOfWork.GetRepository<HolidayException, int>();
            var holiday = await repository.GetAsync(id);
            if (holiday is null)
                throw new HolidayExceptionNotFoundException(id);
            if (!holiday.IsActive)
                return $"Holiday exception with Id [{id}] was already removed.";

            holiday.IsActive = false;
            holiday.RemovedAt = DateTime.UtcNow;
            repository.Update(holiday);
            await unitOfWork.SaveChangesAsync();
            return $"Holiday exception with Id [{id}] was removed successfully.";
        }

        private static string Snapshot(ScheduledLesson lesson)
            => JsonSerializer.Serialize(new
            {
                lesson.WeeklyScheduleId,
                lesson.Date,
                lesson.StartTime,
                lesson.EndTime,
                lesson.SubjectId,
                lesson.TeacherId,
                lesson.Status
            });

        private static ScheduleChangeLog CreateChangeLog(
            ScheduledLesson lesson,
            ScheduleChangeOperationType operationType,
            ScheduleChangeSource source,
            string? reason,
            string previousValues,
            string newValues)
            => new()
            {
                ScheduledLessonId = lesson.Id,
                ScheduledLesson = lesson,
                OperationType = operationType,
                Timestamp = DateTime.UtcNow,
                Reason = reason,
                Source = source,
                PreviousValues = previousValues,
                NewValues = newValues
            };
    }
}
