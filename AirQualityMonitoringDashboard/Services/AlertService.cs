using AirQualityMonitoringDashboard.Data;
using AirQualityMonitoringDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirQualityMonitoringDashboard.Services
{
    public class AlertService
    {
        private readonly ApplicationDbContext _context;
        private Dictionary<string, int> _thresholds;

        public AlertService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;

            // Initialize with default thresholds, but allow them to be overridden by app settings
            _thresholds = new Dictionary<string, int>
            {
                { "moderate", 51 },
                { "unhealthySensitive", 101 },
                { "unhealthy", 151 },
                { "veryUnhealthy", 201 },
                { "hazardous", 301 }
            };

            // Try to load thresholds from configuration if available
            var configThresholds = configuration.GetSection("AQIAlerts:Thresholds");
            if (configThresholds.Exists())
            {
                foreach (var key in _thresholds.Keys.ToList())
                {
                    var configValue = configThresholds[key];
                    if (!string.IsNullOrEmpty(configValue) && int.TryParse(configValue, out int value))
                    {
                        _thresholds[key] = value;
                    }
                }
            }
        }

        public async Task<IEnumerable<Alert>> GetActiveAlertsAsync()
        {
            return await _context.Set<Alert>()
                .Where(a => a.IsActive)
                .Include(a => a.Sensor)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetAlertsForSensorAsync(int sensorId)
        {
            return await _context.Set<Alert>()
                .Where(a => a.SensorId == sensorId && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public Dictionary<string, int> GetThresholds()
        {
            return _thresholds;
        }

        public void UpdateThresholds(Dictionary<string, int> thresholds)
        {
            foreach (var item in thresholds)
            {
                if (_thresholds.ContainsKey(item.Key))
                {
                    _thresholds[item.Key] = item.Value;
                }
            }
        }

        public async Task DismissAlertAsync(int id)
        {
            var alert = await _context.Set<Alert>().FindAsync(id);
            if (alert != null)
            {
                alert.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DismissAllAlertsAsync()
        {
            var activeAlerts = await _context.Set<Alert>()
                .Where(a => a.IsActive)
                .ToListAsync();

            foreach (var alert in activeAlerts)
            {
                alert.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CheckAndCreateAlertsAsync()
        {
            // Get all active sensors with their latest AQI readings
            var sensors = await _context.Sensors
                .Where(s => s.Status == "Active")
                .ToListAsync();

            foreach (var sensor in sensors)
            {
                // Get latest AQI reading for this sensor
                var latestReading = await _context.AQIData
                    .Where(a => a.SensorId == sensor.Id)
                    .OrderByDescending(a => a.RecordedAt)
                    .FirstOrDefaultAsync();

                if (latestReading != null)
                {
                    // Determine alert category and severity
                    var (category, severity) = GetAQICategory(latestReading.AQI);

                    // Skip if AQI is in the "Good" category
                    if (severity == "good")
                        continue;

                    // Check for an existing active alert for this sensor
                    var existingAlert = await _context.Set<Alert>()
                        .Where(a => a.SensorId == sensor.Id && a.IsActive)
                        .FirstOrDefaultAsync();

                    if (existingAlert != null)
                    {
                        // Update existing alert if severity changed
                        if (existingAlert.Severity != severity)
                        {
                            existingAlert.Category = category;
                            existingAlert.Severity = severity;
                            existingAlert.AQI = latestReading.AQI;
                            existingAlert.Message = $"AQI is now {latestReading.AQI} ({category})";
                            existingAlert.CreatedAt = DateTime.UtcNow;

                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        // Create new alert
                        var alert = new Alert
                        {
                            SensorId = sensor.Id,
                            Category = category,
                            Severity = severity,
                            AQI = latestReading.AQI,
                            Message = $"AQI is {latestReading.AQI} ({category})",
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        _context.Add(alert);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        private (string Category, string Severity) GetAQICategory(int aqi)
        {
            if (aqi <= 50) return ("Good", "good");
            if (aqi <= _thresholds["moderate"]) return ("Moderate", "moderate");
            if (aqi <= _thresholds["unhealthySensitive"]) return ("Unhealthy for Sensitive Groups", "unhealthy-sensitive");
            if (aqi <= _thresholds["unhealthy"]) return ("Unhealthy", "unhealthy");
            if (aqi <= _thresholds["veryUnhealthy"]) return ("Very Unhealthy", "very-unhealthy");
            return ("Hazardous", "hazardous");
        }
    }
}