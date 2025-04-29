using AirQualityMonitoringDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AirQualityMonitoringDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AQIDataController : ControllerBase
    {
        private readonly IAQIDataRepository _aqiRepository;

        public AQIDataController(IAQIDataRepository aqiRepository)
        {
            _aqiRepository = aqiRepository;
        }

        // Endpoint to get the latest AQI data for a specific sensor
        [HttpGet("latest/{sensorId}/{topCount}")]
        public async Task<IActionResult> GetLatestAQIData(int sensorId, int topCount)
        {
            var data = await _aqiRepository.GetLatestReadingsAsync(sensorId, topCount);
            return Ok(data);
        }

        // Endpoint for historical data filtering option
        [HttpGet("historical/{sensorId}")]
        public async Task<IActionResult> GetHistoricalData(int sensorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate == default || endDate == default || startDate > endDate)
            {
                return BadRequest("Invalid date range provided.");
            }

            var data = await _aqiRepository.GetHistoricalReadingsAsync(sensorId, startDate, endDate);
            return Ok(data);
        }
    }
}
