using Drivious.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Drivious.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardsController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardsController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            return Ok(await _dashboardService.GetDashboardAsync());
        }
    }
}
