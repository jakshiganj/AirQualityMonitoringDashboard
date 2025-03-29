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
    }
}
