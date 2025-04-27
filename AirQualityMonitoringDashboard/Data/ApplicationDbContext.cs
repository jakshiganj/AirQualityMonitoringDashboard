using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AirQualityMonitoringDashboard.Models;

namespace AirQualityMonitoringDashboard.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>  // Inheriting from IdentityDbContext for User Authentication
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for Air Quality Data
        public DbSet<AQIData> AQIData { get; set; }

        // DbSet for Sensors
        public DbSet<Sensor> Sensors { get; set; }

        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Calls the base class to add Identity-related configurations

            // Define relationships between models if necessary
            modelBuilder.Entity<Sensor>()
                .HasMany(s => s.AQIData)
                .WithOne(a => a.Sensor)
                .HasForeignKey(a => a.SensorId)
                .OnDelete(DeleteBehavior.Cascade);  // Ensure cascading delete of AQIData when Sensor is deleted

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Sensor)
                .WithMany()
                .HasForeignKey(a => a.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
