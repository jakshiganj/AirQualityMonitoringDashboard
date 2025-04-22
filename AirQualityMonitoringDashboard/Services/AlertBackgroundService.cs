using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirQualityMonitoringDashboard.Services
{
    public class AlertBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AlertBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
        private bool _initialRun = true;

        public AlertBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AlertBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Alert Background Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var alertService = scope.ServiceProvider.GetRequiredService<InMemoryAlertService>();

                        // On first run, create some test alerts to ensure the system is working
                        if (_initialRun)
                        {
                            _logger.LogInformation("Initial run - creating test alerts");
                            await alertService.CreateTestAlertsAsync();
                            _initialRun = false;
                        }

                        _logger.LogInformation("Checking for air quality alerts...");
                        await alertService.CheckAndCreateAlertsAsync();

                        // Process good alerts explicitly
                        _logger.LogInformation("Processing good alerts...");
                        await alertService.ProcessGoodAlertsAsync();

                        // Process moderate alerts explicitly
                        _logger.LogInformation("Processing moderate alerts...");
                        await alertService.ProcessModerateAlertsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while checking for alerts");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Normal on shutdown, we can ignore this
                    break;
                }
            }

            _logger.LogInformation("Alert Background Service is stopping");
        }
    }
}