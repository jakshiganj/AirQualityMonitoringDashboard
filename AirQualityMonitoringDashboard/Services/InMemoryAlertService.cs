using AirQualityMonitoringDashboard.Data;
using AirQualityMonitoringDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AirQualityMonitoringDashboard.Services
{

    /// In-memory implementation of the alert service that doesn't require database changes

    public class InMemoryAlertService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InMemoryAlertService> _logger;
        private readonly string _storageFilePath;

        // Default thresholds for different AQI levels
        private Dictionary<string, int> _thresholds;

        // In-memory collection of alerts
        private List<Alert> _activeAlerts = new List<Alert>();

        public InMemoryAlertService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<InMemoryAlertService> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;

            // Set up file path for alert storage
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath); // Ensure directory exists
            _storageFilePath = Path.Combine(appDataPath, "alerts.json");

            // Load any saved alerts
            LoadAlerts();

            // Initialize with default thresholds
            _thresholds = new Dictionary<string, int>
            {
                { "good", 0 }, // Added good category with a threshold of 0
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

            _logger.LogInformation("InMemoryAlertService initialized with {count} alerts", _activeAlerts.Count);
        }

        public Task<IEnumerable<Alert>> GetActiveAlertsAsync()
        {
            // Clean up old alerts (older than 24 hours)
            var cutoff = DateTime.UtcNow.AddHours(-24);
            _activeAlerts.RemoveAll(a => a.CreatedAt < cutoff);

            return Task.FromResult<IEnumerable<Alert>>(_activeAlerts.OrderByDescending(a => a.CreatedAt).ToList());
        }

        public Task<IEnumerable<Alert>> GetAlertsForSensorAsync(int sensorId)
        {
            // Convert the IOrderedEnumerable to List to make it compatible with the return type
            var alerts = _activeAlerts
                .Where(a => a.SensorId == sensorId && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToList(); // This fixes the conversion issue

            return Task.FromResult<IEnumerable<Alert>>(alerts);
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

        public Task DismissAlertAsync(int id)
        {
            var alert = _activeAlerts.FirstOrDefault(a => a.Id == id);
            if (alert != null)
            {
                alert.IsActive = false;
                SaveAlerts();
            }
            return Task.CompletedTask;
        }

        public Task DismissAllAlertsAsync()
        {
            foreach (var alert in _activeAlerts)
            {
                alert.IsActive = false;
            }
            SaveAlerts();
            return Task.CompletedTask;
        }

        public async Task CheckAndCreateAlertsAsync()
        {
            try
            {
                // Create a scope to get the DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Get all active sensors
                    var sensors = await context.Sensors
                        .Where(s => s.Status == "Active")
                        .ToListAsync();

                    foreach (var sensor in sensors)
                    {
                        // Get latest AQI reading for this sensor
                        var latestReading = await context.AQIData
                            .Where(a => a.SensorId == sensor.Id)
                            .OrderByDescending(a => a.RecordedAt)
                            .FirstOrDefaultAsync();

                        if (latestReading != null)
                        {
                            // Determine alert category and severity
                            var (category, severity) = GetAQICategory(latestReading.AQI);



                            // Check for an existing active alert for this sensor
                            var existingAlert = _activeAlerts
                                .FirstOrDefault(a => a.SensorId == sensor.Id && a.IsActive);

                            if (existingAlert != null)
                            {
                                // Update existing alert if severity changed
                                if (existingAlert.Severity != severity)
                                {
                                    _logger.LogInformation("Updating alert for sensor {sensorName} - severity changed to {severity}",
                                        sensor.Name, severity);

                                    existingAlert.Category = category;
                                    existingAlert.Severity = severity;
                                    existingAlert.AQI = latestReading.AQI;
                                    existingAlert.Message = CreateAlertMessage(category, latestReading.AQI);
                                    existingAlert.CreatedAt = DateTime.UtcNow;
                                    existingAlert.SensorName = sensor.Name;
                                    existingAlert.SensorLocation = sensor.Location;

                                    SaveAlerts();
                                }
                            }
                            else
                            {
                                // Create new alert
                                _logger.LogInformation("Creating new alert for sensor {sensorName} - {severity}",
                                    sensor.Name, severity);

                                var alert = new Alert
                                {
                                    Id = GenerateAlertId(),
                                    SensorId = sensor.Id,
                                    Category = category,
                                    Severity = severity,
                                    AQI = latestReading.AQI,
                                    Message = CreateAlertMessage(category, latestReading.AQI),
                                    CreatedAt = DateTime.UtcNow,
                                    IsActive = true,
                                    SensorName = sensor.Name,
                                    SensorLocation = sensor.Location
                                };

                                _activeAlerts.Add(alert);
                                SaveAlerts();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAndCreateAlertsAsync");
            }
        }

        // Helper methods

        private (string Category, string Severity) GetAQICategory(int aqi)
        {
            if (aqi <= 50) return ("Good", "good");
            if (aqi <= _thresholds["moderate"]) return ("Moderate", "moderate");
            if (aqi <= _thresholds["unhealthySensitive"]) return ("Unhealthy for Sensitive Groups", "unhealthy-sensitive");
            if (aqi <= _thresholds["unhealthy"]) return ("Unhealthy", "unhealthy");
            if (aqi <= _thresholds["veryUnhealthy"]) return ("Very Unhealthy", "very-unhealthy");
            return ("Hazardous", "hazardous");
        }

        private string CreateAlertMessage(string category, int aqi)
        {
            switch (category.ToLower())
            {
                case "good":
                    return $"Good air quality: AQI is {aqi}";
                case "moderate":
                    return $"Moderate air quality alert: AQI is {aqi}";
                case "unhealthy for sensitive groups":
                    return $"Unhealthy for sensitive groups: AQI is {aqi}";
                case "unhealthy":
                    return $"Unhealthy air quality: AQI is {aqi}";
                case "very unhealthy":
                    return $"Very unhealthy air quality: AQI is {aqi}";
                case "hazardous":
                    return $"Hazardous air quality: AQI is {aqi}";
                default:
                    return $"AQI is {aqi} ({category})";
            }
        }

        private int GenerateAlertId()
        {
            // Simple ID generation - find the max existing ID and add 1
            if (_activeAlerts.Count == 0)
                return 1;

            return _activeAlerts.Max(a => a.Id) + 1;
        }

        private void SaveAlerts()
        {
            try
            {
                var json = JsonSerializer.Serialize(_activeAlerts);
                File.WriteAllText(_storageFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving alerts");
            }
        }

        private void LoadAlerts()
        {
            try
            {
                if (File.Exists(_storageFilePath))
                {
                    var json = File.ReadAllText(_storageFilePath);
                    var alerts = JsonSerializer.Deserialize<List<Alert>>(json);
                    if (alerts != null)
                    {
                        _activeAlerts = alerts;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading alerts");
                _activeAlerts = new List<Alert>();
            }
        }

        public async Task ProcessGoodAlertsAsync()
        {
            try
            {
                // Create a scope to get the DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Get all active sensors
                    var sensors = await context.Sensors
                        .Where(s => s.Status == "Active")
                        .ToListAsync();

                    foreach (var sensor in sensors)
                    {
                        // Get latest AQI reading for this sensor
                        var latestReading = await context.AQIData
                            .Where(a => a.SensorId == sensor.Id)
                            .OrderByDescending(a => a.RecordedAt)
                            .FirstOrDefaultAsync();

                        if (latestReading != null)
                        {
                            // Check specifically for good category
                            if (latestReading.AQI >= 0 && latestReading.AQI <= 50)
                            {
                                // Check if there's already a good alert for this sensor
                                var existingAlert = _activeAlerts
                                    .FirstOrDefault(a => a.SensorId == sensor.Id && a.IsActive && a.Severity == "good");

                                if (existingAlert == null)
                                {
                                    _logger.LogInformation(
                                        "Creating new good alert for sensor {sensorName} - AQI: {aqi}",
                                        sensor.Name, latestReading.AQI);

                                    // Create new good alert
                                    var alert = new Alert
                                    {
                                        Id = GenerateAlertId(),
                                        SensorId = sensor.Id,
                                        Category = "Good",
                                        Severity = "good",
                                        AQI = latestReading.AQI,
                                        Message = $"Good air quality: AQI is {latestReading.AQI}",
                                        CreatedAt = DateTime.UtcNow,
                                        IsActive = true,
                                        SensorName = sensor.Name,
                                        SensorLocation = sensor.Location
                                    };

                                    _activeAlerts.Add(alert);
                                    SaveAlerts();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error in ProcessGoodAlertsAsync");
            }
        }

        public async Task ProcessModerateAlertsAsync()
        {
            try
            {
                // Create a scope to get the DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Get all active sensors
                    var sensors = await context.Sensors
                        .Where(s => s.Status == "Active")
                        .ToListAsync();

                    foreach (var sensor in sensors)
                    {
                        // Get latest AQI reading for this sensor
                        var latestReading = await context.AQIData
                            .Where(a => a.SensorId == sensor.Id)
                            .OrderByDescending(a => a.RecordedAt)
                            .FirstOrDefaultAsync();

                        if (latestReading != null)
                        {
                            // Check specifically for moderate category
                            if (latestReading.AQI > 50 && latestReading.AQI <= _thresholds["moderate"])
                            {
                                // Check if there's already a moderate alert for this sensor
                                var existingAlert = _activeAlerts
                                    .FirstOrDefault(a => a.SensorId == sensor.Id && a.IsActive && a.Severity == "moderate");

                                if (existingAlert == null)
                                {
                                    _logger.LogInformation(
                                        "Creating new moderate alert for sensor {sensorName} - AQI: {aqi}",
                                        sensor.Name, latestReading.AQI);

                                    // Create new moderate alert
                                    var alert = new Alert
                                    {
                                        Id = GenerateAlertId(),
                                        SensorId = sensor.Id,
                                        Category = "Moderate",
                                        Severity = "moderate",
                                        AQI = latestReading.AQI,
                                        Message = $"Moderate AQI level: {latestReading.AQI}",
                                        CreatedAt = DateTime.UtcNow,
                                        IsActive = true,
                                        SensorName = sensor.Name,
                                        SensorLocation = sensor.Location
                                    };

                                    _activeAlerts.Add(alert);
                                    SaveAlerts();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
 
                _logger.LogError(ex, "Error in ProcessModerateAlertsAsync");
            }
        }

        // Method to force create test alerts for each category 
        public Task CreateTestAlertsAsync()
        {
            var testSensorId = 1; // Use a valid sensor ID from your database

            // Create a test alert for each severity level
            var alerts = new List<Alert>
            {
                new Alert
                {
                    Id = GenerateAlertId(),
                    SensorId = testSensorId,
                    Category = "Good",
                    Severity = "good",
                    AQI = 30,
                    Message = "Good air quality: AQI is 30",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    SensorName = "Test Sensor",
                    SensorLocation = "Test Location"
                },
                new Alert
                {
                    Id = GenerateAlertId(),
                    SensorId = testSensorId,
                    Category = "Moderate",
                    Severity = "moderate",
                    AQI = 75,
                    Message = "Moderate air quality alert: AQI is 75",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    SensorName = "Test Sensor",
                    SensorLocation = "Test Location"
                },
                new Alert
                {
                    Id = GenerateAlertId(),
                    SensorId = testSensorId,
                    Category = "Unhealthy for Sensitive Groups",
                    Severity = "unhealthy-sensitive",
                    AQI = 125,
                    Message = "Unhealthy for sensitive groups: AQI is 125",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    SensorName = "Test Sensor",
                    SensorLocation = "Test Location"
                }
            };

            _activeAlerts.AddRange(alerts);
            SaveAlerts();

            _logger.LogInformation("Created {count} test alerts", alerts.Count);

            return Task.CompletedTask;
        }
    }
}