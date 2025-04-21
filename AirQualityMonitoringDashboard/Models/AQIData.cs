using System;

namespace AirQualityMonitoringDashboard.Models
{
    public class AQIData
    {
        public int Id { get; set; }  // Primary key
        public int SensorId { get; set; }  // Foreign key linking to Sensor
        public int AQI { get; set; }  
        public float? PM10 { get; set; }
        public float? PM25 { get; set; }
        public float? CO { get; set; }
        public float? NO2 { get; set; }
        public float? O3 { get; set; }
        public float? SO2 { get; set; }
        public float? Temperature { get; set; }
        public float? Humidity { get; set; }
        public float? Pressure { get; set; }
        public float? WindSpeed { get; set; }
        public DateTime RecordedAt { get; set; }  // Timestamp of the AQI reading

        // Navigation property
        public Sensor Sensor { get; set; }
    }
}
