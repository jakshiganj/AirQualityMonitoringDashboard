using System.Collections.Generic;
using System.Threading.Tasks;
using AirQualityMonitoringDashboard.Models;

namespace AirQualityMonitoringDashboard.Repositories
{
    public interface IAQIDataRepository
    {
        Task<IEnumerable<AQIData>> GetAllReadingsAsync();
        Task<AQIData> GetReadingByIdAsync(int id);
        Task AddReading(AQIData reading);

        // Add GetLatestReadings method to retrieve the latest readings (you can adjust the return type as needed)
        Task<IEnumerable<AQIData>> GetLatestReadingsAsync(int sensorId, int topCount);  // Example: Get latest readings for a sensor
    }
}
