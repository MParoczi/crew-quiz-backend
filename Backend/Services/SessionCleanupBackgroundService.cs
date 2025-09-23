using Backend.Interfaces.Services;
using Backend.Models.Configurations;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class SessionCleanupBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<AppSettings> apiSettings) : BackgroundService
{
    private readonly AppSettings _appSettings = apiSettings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sessionTimeoutHours = _appSettings.SessionCleanup.SessionTimeoutHours;
        var cleanupInterval = TimeSpan.FromHours(_appSettings.SessionCleanup.IntervalHours);

        // Wait for the application to fully start before beginning cleanup operations
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupOperationAsync();
            }
            catch (Exception ex)
            {
                // Continue with next iteration on error
            }

            try
            {
                await Task.Delay(cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PerformCleanupOperationAsync()
    {
        var sessionTimeoutHours = _appSettings.SessionCleanup.SessionTimeoutHours;
        using var scope = serviceProvider.CreateScope();
        var sessionCleanupService = scope.ServiceProvider.GetRequiredService<ISessionCleanupService>();

        await sessionCleanupService.PerformFullCleanupAsync(sessionTimeoutHours);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }
}