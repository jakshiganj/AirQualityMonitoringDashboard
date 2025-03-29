using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirQualityMonitoringDashboard.Data;
using AirQualityMonitoringDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace AirQualityMonitoringDashboard.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        private readonly ApplicationDbContext _context;

        public SensorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sensor>> GetAllSensorsAsync()
        {
            return await _context.Sensors.ToListAsync();
        }

        public async Task<Sensor> GetSensorByIdAsync(int id)
        {
            return await _context.Sensors.FindAsync(id);
        }

        public async Task AddSensorAsync(Sensor sensor)
        {
            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSensorAsync(Sensor sensor)
        {
            _context.Sensors.Update(sensor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSensorAsync(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor != null)
            {
                _context.Sensors.Remove(sensor);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ FIX: Implement the missing GetSensorByLocation method
        public async Task<Sensor> GetSensorByLocation(double latitude, double longitude)
        {
            return await _context.Sensors
                .FirstOrDefaultAsync(s => s.Latitude == latitude && s.Longitude == longitude);
        }


    }
}
