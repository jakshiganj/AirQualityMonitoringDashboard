using System.Collections.Generic;
using System.Threading.Tasks;
using AirQualityMonitoringDashboard.Models;

namespace AirQualityMonitoringDashboard.Repositories
{
    public interface ISensorRepository
    {
        Task<IEnumerable<Sensor>> GetAllSensorsAsync();
        Task<Sensor> GetSensorByLocation(double latitude, double longitude);
        Task<Sensor> GetSensorById(string sensorId);
        Task AddSensorAsync(Sensor sensor);
        Task UpdateSensorAsync(Sensor sensor);
        Task DeleteSensorAsync(int id);
    }
}
