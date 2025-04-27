using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AirQualityMonitoringDashboard.Models
{
    /// Represents an air quality alert

    public class Alert
    {
        public int Id { get; set; }

        public int SensorId { get; set; }

        public string Category { get; set; }

        public string Severity { get; set; }

        public int AQI { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property - use JsonIgnore to prevent serialization issues
        [JsonIgnore]
        public Sensor Sensor { get; set; }

        // Additional properties to include sensor information without requiring DB relationship
        public string SensorName { get; set; }
        public string SensorLocation { get; set; }
    }
}