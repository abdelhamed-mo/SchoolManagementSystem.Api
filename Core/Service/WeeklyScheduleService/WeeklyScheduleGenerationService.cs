using Microsoft.Extensions.Logging;
using Service.ConcreteSpecifications;

namespace Service.WeeklyScheduleService
{
    public class WeeklyScheduleGenerationService(
        IUnitOfWork unitOfWork,
        IWeeklyScheduleService weeklyScheduleService,
        ILogger<WeeklyScheduleGenerationService> logger) : IWeeklyScheduleGenerationService
    {
        public async Task GenerateCurrentAcademicWeeksAsync(CancellationToken cancellationToken)
        {
            var academicYear = await unitOfWork.GetRepository<AcademicYear, int>()
                .GetAsync(new AcademicYearSpecifications(true));
            var semester = await unitOfWork.GetRepository<Semester, int>()
                .GetAsync(new SemesterSpecifications(true));
            logger.LogInformation(
    "AcademicYear: {YearId}, Semester: {SemesterId}",
    academicYear?.Id,
    semester?.Id);
            if (academicYear is null || semester is null || semester.AcademicYearId != academicYear.Id)
            {
                logger.LogWarning("Weekly schedule generation skipped because the active academic context is incomplete.");
                return;
            }

            var today = SchoolTime.TodayInCairo();
            var firstWeek = SchoolTime.SundayOfWeek(semester.StartDate);
            var lastWeek = SchoolTime.SundayOfWeek(semester.EndDate);
            var currentWeek = SchoolTime.SundayOfWeek(today);
            var throughWeek = currentWeek < lastWeek ? currentWeek : lastWeek;
            if (throughWeek < firstWeek)
                return;

            var classes = await unitOfWork.GetRepository<Class, int>()
                .GetAllAsync(new ClassSchedulingSpecifications(academicYear.Id));
            foreach (var classEntity in classes)
            {
                for (var week = firstWeek; week <= throughWeek; week = week.AddDays(7))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        await weeklyScheduleService.GenerateAsync(classEntity.Id, week);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception,
                            "Weekly schedule generation failed for Class {ClassId} and week {WeekStartDate}.",
                            classEntity.Id,
                            week);
                    }
                }
            }
        }
    }
}
