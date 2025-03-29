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

        public DateTime CreatedAt { get; set; } 

        public ICollection<AQIData> AQIData { get; set; }  // This allows access to all AQI data for this sensor
    }
}
