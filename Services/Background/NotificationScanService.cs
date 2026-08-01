using Drivious.Models;
using Drivious.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Drivious.Services.Background
{
    /// <summary>
    /// Runs the notification generator on a timer. The generator itself is scoped
    /// because it uses the database context, so every pass takes its own scope.
    /// </summary>
    public class NotificationScanService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly NotificationSettings _settings;
        private readonly ILogger<NotificationScanService> _logger;

        public NotificationScanService(
            IServiceProvider services,
            IOptions<NotificationSettings> settings,
            ILogger<NotificationScanService> logger)
        {
            _services = services;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("Notification scanning is disabled by configuration.");
                return;
            }

            var interval = TimeSpan.FromHours(Math.Max(_settings.ScanIntervalHours, 0.1));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();

                    var generator = scope.ServiceProvider.GetRequiredService<INotificationGenerator>();

                    await generator.GenerateAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // A failed pass must not take the service down - the next one may
                    // well succeed, and the fleet still needs the following warnings.
                    _logger.LogError(ex, "Notification scan failed; retrying at the next interval.");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
