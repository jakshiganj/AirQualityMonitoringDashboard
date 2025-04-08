using Microsoft.AspNetCore.Mvc;

namespace AirQualityMonitoringDashboard.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
