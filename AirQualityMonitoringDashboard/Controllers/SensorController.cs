using AirQualityMonitoringDashboard.Models;
using AirQualityMonitoringDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AirQualityMonitoringDashboard.Controllers
{
    public class SensorController : Controller
    {
        private readonly ISensorRepository _sensorRepository;

        public SensorController(ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        public async Task<IActionResult> Manage()
        {
            var sensors = await _sensorRepository.GetAllSensorsAsync();
            return View(sensors);
        }

        // Add Sensor
        [HttpPost]
        public async Task<IActionResult> Add(Sensor sensor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            sensor.AQIData ??= new List<AQIData>();
            await _sensorRepository.AddSensorAsync(sensor);
            return RedirectToAction("Manage"); // Reload the page after adding
        }

        // Delete Sensor (Now uses POST)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var sensor = await _sensorRepository.GetSensorByIdAsync(id);
            if (sensor == null)
            {
                return NotFound(new { message = "Sensor not found" });
            }

            await _sensorRepository.DeleteSensorAsync(id);
            return RedirectToAction("Manage"); // Reload the page after deletion
        }

        [HttpGet]
        public async Task<IActionResult> GetSensor(int id)
        {
            var sensor = await _sensorRepository.GetSensorByIdAsync(id);
            if (sensor == null)
            {
                return NotFound(new { message = "Sensor not found" });
            }

            return Json(sensor);
        }

        // Edit Sensor
        [HttpPost] // Changed to POST for MVC convention
        public async Task<IActionResult> Edit(Sensor sensor)
        {
            var sensor_selected = await _sensorRepository.GetSensorByIdAsync(sensor.Id);
            if (sensor_selected == null)
            {
                return NotFound(new { message = "Sensor not found" });
            }

            sensor_selected.Name = sensor.Name;
            sensor_selected.Location = sensor.Location;
            sensor_selected.Status = sensor.Status; 

            await _sensorRepository.UpdateSensorAsync(sensor_selected);
            return RedirectToAction("Manage"); // Reload the page after editing
        }
    }
}
