using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SchoolMAnagementSystem.Api
{
    public sealed class WeeklyScheduleGenerationHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<WeeklyScheduleGenerationHostedService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Weekly Schedule Hosted Service Started.***********************************************************");

            await RunCatchUpAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await RunCatchUpAsync(stoppingToken);
        }

        private async Task RunCatchUpAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var generationService = scope.ServiceProvider.GetRequiredService<IServiceManager>().WeeklyScheduleGenerationService;
                await generationService.GenerateCurrentAcademicWeeksAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Weekly schedule catch-up generation failed.");
            }
        }
    }
}
