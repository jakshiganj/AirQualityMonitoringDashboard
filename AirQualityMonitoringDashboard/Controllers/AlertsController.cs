using AirQualityMonitoringDashboard.Models;
using AirQualityMonitoringDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirQualityMonitoringDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly InMemoryAlertService _alertService;
        private readonly ILogger<AlertController> _logger;

        public AlertController(InMemoryAlertService alertService, ILogger<AlertController> logger)
        {
            _alertService = alertService;
            _logger = logger;
        }

        // GET: api/Alert
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alert>>> GetActiveAlerts()
        {
            try
            {
                var alerts = await _alertService.GetActiveAlertsAsync();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active alerts");
                return StatusCode(500, new { message = "An error occurred while retrieving alerts" });
            }
        }

        // GET: api/Alert/sensor/5
        [HttpGet("sensor/{sensorId}")]
        public async Task<ActionResult<IEnumerable<Alert>>> GetAlertsForSensor(int sensorId)
        {
            try
            {
                var alerts = await _alertService.GetAlertsForSensorAsync(sensorId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for sensor {sensorId}", sensorId);
                return StatusCode(500, new { message = $"An error occurred while retrieving alerts for sensor {sensorId}" });
            }
        }

        // GET: api/Alert/thresholds
        [HttpGet("thresholds")]
        public ActionResult<object> GetAlertThresholds()
        {
            try
            {
                var thresholds = _alertService.GetThresholds();
                return Ok(thresholds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert thresholds");
                return StatusCode(500, new { message = "An error occurred while retrieving alert thresholds" });
            }
        }

        // POST: api/Alert/thresholds
        [HttpPost("thresholds")]
        public ActionResult UpdateAlertThresholds([FromBody] Dictionary<string, int> thresholds)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _alertService.UpdateThresholds(thresholds);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert thresholds");
                return StatusCode(500, new { message = "An error occurred while updating alert thresholds" });
            }
        }

        // POST: api/Alert/dismiss/5
        [HttpPost("dismiss/{id}")]
        public async Task<ActionResult> DismissAlert(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _alertService.DismissAlertAsync(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing alert {alertId}", id);
                return StatusCode(500, new { message = $"An error occurred while dismissing alert {id}" });
            }
        }

        // POST: api/Alert/dismissAll
        [HttpPost("dismissAll")]
        public async Task<ActionResult> DismissAllAlerts()
        {
            try
            {
                await _alertService.DismissAllAlertsAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing all alerts");
                return StatusCode(500, new { message = "An error occurred while dismissing all alerts" });
            }
        }

        // Trigger a manual alert check
        [HttpPost("check")]
        public async Task<ActionResult> TriggerAlertCheck()
        {
            try
            {
                await _alertService.CheckAndCreateAlertsAsync();
                await _alertService.ProcessGoodAlertsAsync();
                await _alertService.ProcessModerateAlertsAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering alert check");
                return StatusCode(500, new { message = "An error occurred while checking for alerts" });
            }
        }

        // Create test alerts for debugging
        [HttpPost("test")]
        public async Task<ActionResult> CreateTestAlerts()
        {
            try
            {
                await _alertService.CreateTestAlertsAsync();
                return Ok(new { success = true, message = "Test alerts created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test alerts");
                return StatusCode(500, new { message = "An error occurred while creating test alerts" });
            }
        }
    }
}