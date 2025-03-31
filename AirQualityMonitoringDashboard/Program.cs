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

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Sensor}/{action=Manage}/{id?}");
app.MapControllers();
app.Run();
