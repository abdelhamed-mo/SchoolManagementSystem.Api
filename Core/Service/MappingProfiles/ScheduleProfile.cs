using AutoMapper;
using Domain.Entities;
using Shared;

namespace Service.MappingProfiles
{
    public class ScheduleProfile : Profile
    {
        public ScheduleProfile()
        {
            CreateMap<BaseTimetable, BaseTimetableDto>();
            CreateMap<BaseTimetablePeriod, BaseTimetablePeriodDto>();
            CreateMap<BaseTimetableEntry, BaseTimetableEntryDto>()
                .ForMember(destination => destination.PeriodNumbers,
                    options => options.MapFrom(source => source.Periods
                        .Where(period => period.BaseTimetablePeriod.PeriodNumber.HasValue)
                        .Select(period => period.BaseTimetablePeriod.PeriodNumber!.Value)));
            CreateMap<WeeklySchedulePeriod, WeeklySchedulePeriodDto>();
            CreateMap<ScheduledLessonPeriod, ScheduledLessonPeriodDto>();
            CreateMap<WeeklySchedule, WeeklyScheduleDto>();
            CreateMap<ScheduledLesson, ScheduledLessonDto>()
                .ForMember(destination => destination.Status,
                    options => options.MapFrom(source => (ScheduledLessonStatusDto)(int)source.Status));
            CreateMap<HolidayException, HolidayExceptionDto>();
            CreateMap<ScheduleChangeLog, ScheduleChangeLogDto>()
                .ForMember(destination => destination.OperationType,
                    options => options.MapFrom(source => source.OperationType.ToString()))
                .ForMember(destination => destination.Source,
                    options => options.MapFrom(source => source.Source.ToString()));
        }
    }
}
