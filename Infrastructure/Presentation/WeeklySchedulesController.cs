using Microsoft.AspNetCore.Mvc;
using Service;
using ServiceAbstraction;
using Shared;

namespace Presentation
{
    public class WeeklySchedulesController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet("class/{classId:int}")]
        public async Task<ActionResult<WeeklyScheduleDto>> GetByClassAndWeek(
            int classId,
            [FromQuery] DateTime? weekStartDate)
            => await serviceManager.WeeklyScheduleService.GetByClassAndWeekAsync(classId, weekStartDate ?? SchoolTime.TodayInCairo());

        [HttpPost("class/{classId:int}/generate")]
        public async Task<ActionResult<WeeklyScheduleDto>> Generate(
            int classId,
            [FromQuery] DateTime weekStartDate)
            => await serviceManager.WeeklyScheduleService.GenerateAsync(classId, weekStartDate);

        [HttpGet("{weeklyScheduleId:int}/lessons")]
        public async Task<ActionResult<IEnumerable<ScheduledLessonDto>>> GetLessons(int weeklyScheduleId)
            => new ActionResult<IEnumerable<ScheduledLessonDto>>(
                (await serviceManager.WeeklyScheduleService.GetLessonsAsync(weeklyScheduleId)).ToList());

        [HttpPut("lessons")]
        public async Task<ActionResult<ScheduledLessonDto>> UpdateLesson([FromBody] UpdateScheduledLessonDto dto)
            => await serviceManager.WeeklyScheduleService.UpdateLessonAsync(dto);

        [HttpPost("lessons")]
        public async Task<ActionResult<ScheduledLessonDto>> AddNewLesson([FromBody] NewScheduledLessonDto dto)
            => await serviceManager.WeeklyScheduleService.AddNewLessonAsync(dto);

        [HttpPut("lessons/cancel")]
        public async Task<ActionResult<ScheduledLessonDto>> CancelLesson([FromBody] CancelScheduledLessonDto dto)
            => await serviceManager.WeeklyScheduleService.CancelLessonAsync(dto);

        [HttpGet("lessons/{scheduledLessonId:int}/history")]
        public async Task<ActionResult<IEnumerable<ScheduleChangeLogDto>>> GetLessonHistory(int scheduledLessonId)
            => new ActionResult<IEnumerable<ScheduleChangeLogDto>>(
                (await serviceManager.WeeklyScheduleService.GetLessonHistoryAsync(scheduledLessonId)).ToList());
    }
}
