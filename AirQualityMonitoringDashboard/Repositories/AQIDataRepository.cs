using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AirQualityMonitoringDashboard.Models;
using AirQualityMonitoringDashboard.Data;

namespace AirQualityMonitoringDashboard.Repositories
{
    public class AQIDataRepository : IAQIDataRepository
    {
        private readonly ApplicationDbContext _context;

        public AQIDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddReading(AQIData reading)
        {
            _context.AQIData.Add(reading);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AQIData>> GetAllReadingsAsync()
        {
            return await _context.AQIData.ToListAsync();
        }

        public async Task<AQIData> GetReadingByIdAsync(int id)
        {
            return await _context.AQIData.FindAsync(id);
        }

        // Implement GetLatestReadingsAsync method
        public async Task<IEnumerable<AQIData>> GetLatestReadingsAsync(int sensorId, int topCount)
        {
            return await _context.AQIData
                                 .Where(r => r.SensorId == sensorId)  // Filter by SensorId
                                 .OrderByDescending(r => r.RecordedAt)  // Sort by RecordedAt in descending order
                                 .Take(topCount)  // Get the top 'topCount' readings
                                 .ToListAsync();
        }
    }
}
