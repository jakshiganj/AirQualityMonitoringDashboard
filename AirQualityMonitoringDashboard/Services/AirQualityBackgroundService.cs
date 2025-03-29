using AirQualityMonitoringDashboard.Services;

public class AirQualityBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AirQualityBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var aqiService = scope.ServiceProvider.GetRequiredService<AQIDataService>();
                await aqiService.FetchAndStoreAirQualityData();
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
