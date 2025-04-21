using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using AirQualityMonitoringDashboard.Models;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole(adminRole));

        var adminEmail = "admin@airquality.lk";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, adminRole);
            }
        }
    }
}
