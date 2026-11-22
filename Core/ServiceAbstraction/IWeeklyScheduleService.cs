using Shared;

namespace ServiceAbstraction
{
    public interface IWeeklyScheduleService
    {
        Task<WeeklyScheduleDto> GenerateAsync(int classId, DateTime weekStartDate);
        Task<WeeklyScheduleDto> GetByClassAndWeekAsync(int classId, DateTime weekStartDate);
        Task<IEnumerable<ScheduledLessonDto>> GetLessonsAsync(int weeklyScheduleId);
        Task<ScheduledLessonDto> UpdateLessonAsync(UpdateScheduledLessonDto dto);
        Task<ScheduledLessonDto> AddNewLessonAsync(NewScheduledLessonDto dto);
        Task<ScheduledLessonDto> CancelLessonAsync(CancelScheduledLessonDto dto);
        Task<IEnumerable<ScheduleChangeLogDto>> GetLessonHistoryAsync(int scheduledLessonId);
    }
}
