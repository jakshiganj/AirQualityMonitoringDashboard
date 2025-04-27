using AirQualityMonitoringDashboard.Repositories;
using AirQualityMonitoringDashboard.Services;
using AirQualityMonitoringDashboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AirQualityMonitoringDashboard.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Database Context with correct connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Scoped services
builder.Services.AddScoped<IAQIDataRepository, AQIDataRepository>();
builder.Services.AddScoped<ISensorRepository, SensorRepository>();

// Register other services
builder.Services.AddHttpClient<AQIDataService>();
builder.Services.AddHostedService<AirQualityBackgroundService>();

builder.Services.AddSingleton<InMemoryAlertService>();
builder.Services.AddHostedService<AlertBackgroundService>();
// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "dashboard",
//    pattern: "dashboard/{action=Index}/{id?}",
//    defaults: new { controller = "Dashboard" }
//);

app.MapControllerRoute(
    name: "login",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Optional: manual Dashboard route (not strictly needed)
app.MapControllerRoute(
    name: "dashboard",
    pattern: "dashboard/{action=Index}/{id?}",
    defaults: new { controller = "Dashboard" }
);
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbSeeder.SeedAdminAsync(services);
}

app.Run();
