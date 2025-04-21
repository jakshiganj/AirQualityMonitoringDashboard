using AirQualityMonitoringDashboard.Models;
using AirQualityMonitoringDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AirQualityMonitoringDashboard.Controllers
{

    public class DashboardController : Controller
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly IAQIDataRepository _aqiRepository;

        public DashboardController(ISensorRepository sensorRepository, IAQIDataRepository aqiRepository)
        {
            _sensorRepository = sensorRepository;
            _aqiRepository = aqiRepository;
        }

        public async Task<IActionResult> Index()
        {
            var sensors = await _sensorRepository.GetAllSensorsAsync();
            return View(sensors);
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestAQIData(int sensorId, int count = 24)
        {
            var data = await _aqiRepository.GetLatestReadingsAsync(sensorId, count);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActiveSensors()
        {
            var sensors = await _sensorRepository.GetAllSensorsAsync();
            return Json(sensors);
        }
    }
}