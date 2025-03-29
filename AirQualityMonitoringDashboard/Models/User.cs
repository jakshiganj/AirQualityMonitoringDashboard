using Microsoft.AspNetCore.Identity;

namespace AirQualityMonitoringDashboard.Models
{
    public class User : IdentityUser
    {
        // You can add additional properties if needed
        public string FullName { get; set; }
    }
}
