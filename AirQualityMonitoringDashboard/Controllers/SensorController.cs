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

        //Add Sensor
        public async Task<IActionResult> Add([FromBody] Sensor sensor)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _sensorRepository.AddSensorAsync(sensor);
            return View(sensor);
        }

        //Delete Sensor
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var sensor_id = _sensorRepository.GetSensorById(id);
            if ( sensor_id == null)
            {
                return NotFound();
            }

            await _sensorRepository.DeleteSensorAsync(id);
            return Ok();
        }

        //Edit Sensor
        public async Task<IActionResult> GetSensor(int id)
        {
            var sensor_id = _sensorRepository.GetSensorById(id);
            if (sensor_id == null)
            {
                return NotFound();
            }

            return Json(sensor_id);
        }

        public async Task<IActionResult> Edit([FromBody] Sensor sensor)
        {
            var sensor_selected = _sensorRepository.GetSensorById(sensor.Id);
            if (sensor_selected == null)
            {
                return NotFound();
            }

            sensor_selected.Name = sensor.Name;
            sensor_selected.Location = sensor.Location;

            await _sensorRepository.UpdateSensorAsync(sensor_selected);
            return Ok(sensor_selected);
        }
    }
}
