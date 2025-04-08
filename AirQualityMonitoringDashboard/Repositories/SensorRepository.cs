using System;
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

        // ✅ Fetch all sensors without tracking (prevents update conflicts)
        public async Task<IEnumerable<Sensor>> GetAllSensorsAsync()
        {
            return await _context.Sensors.AsNoTracking().ToListAsync();
        }

        // ✅ Get sensor by ID with exception handling
        public async Task<Sensor> GetSensorByIdAsync(int id)
        {
            try
            {
                return await _context.Sensors.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching sensor: {ex.Message}");
                return null;
            }
        }

        // ✅ Ensure CreatedAt is set before adding
        public async Task AddSensorAsync(Sensor sensor)
        {
            if (sensor.CreatedAt == default)
            {
                sensor.CreatedAt = DateTime.UtcNow;
            }

            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();
        }

        // ✅ Fix tracking issue when updating sensor
        public async Task UpdateSensorAsync(Sensor sensor)
        {
            var existingSensor = await _context.Sensors.FindAsync(sensor.Id);
            if (existingSensor == null)
            {
                throw new KeyNotFoundException("Sensor not found.");
            }

            _context.Entry(existingSensor).CurrentValues.SetValues(sensor);
            await _context.SaveChangesAsync();
        }

        // ✅ Fetch sensor before deleting to prevent tracking issues
        public async Task DeleteSensorAsync(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor != null)
            {
                _context.Sensors.Remove(sensor);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Sensor not found.");
            }
        }

        // ✅ Fetch sensor by location
        public async Task<Sensor> GetSensorByLocation(double latitude, double longitude)
        {
            return await _context.Sensors
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Latitude == latitude && s.Longitude == longitude);
        }
    }
}
