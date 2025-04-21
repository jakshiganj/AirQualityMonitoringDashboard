using AirQualityMonitoringDashboard.Data;
using AirQualityMonitoringDashboard.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            return await _context.Sensors
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Sensor> GetSensorByLocation(double latitude, double longitude)
        {
            return await _context.Sensors
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Latitude == latitude && s.Longitude == longitude);
        }

        public async Task<Sensor> GetSensorByIdAsync(int sensorId)
        {
            return await _context.Sensors
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sensorId);
        }

        public async Task AddSensorAsync(Sensor sensor)
        {
            if (sensor.CreatedAt == default)
            {
                sensor.CreatedAt = DateTime.UtcNow;
            }

            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();
        }

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
    }
}