using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirQualityMonitoringDashboard.Models
{
    public class Sensor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public string Location { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; } = "Active"; // Default value is 'Active'

        public DateTime CreatedAt { get; set; } // Default to current time

        public ICollection<AQIData> AQIData { get; set; }  // This allows access to all AQI data for this sensor
    }
}
