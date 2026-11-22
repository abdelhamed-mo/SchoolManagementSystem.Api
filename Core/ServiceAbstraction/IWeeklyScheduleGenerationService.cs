namespace ServiceAbstraction
{
    public interface IWeeklyScheduleGenerationService
    {
        Task GenerateCurrentAcademicWeeksAsync(CancellationToken cancellationToken);
    }
}
