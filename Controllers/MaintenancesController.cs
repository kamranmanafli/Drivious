using Drivious.DTOs.Maintenance;
using Drivious.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Drivious.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenancesController : ControllerBase
    {
        private readonly IMaintenanceService _service;

        public MaintenancesController(IMaintenanceService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(MaintenanceCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);

            return StatusCode(result.Success ? StatusCodes.Status201Created : StatusCodes.Status400BadRequest, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            var result = await _service.RemoveAsync(id);

            return StatusCode(result.Success ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _service.GetAllAsync();

            return StatusCode(result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetAsync(id);

            return StatusCode(result.Success ? StatusCodes.Status200OK : StatusCodes.Status404NotFound, result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, MaintenanceUpdateDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            return StatusCode(result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, result);
        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            var result = await _service.ToggleAsync(id);

            return StatusCode(result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, result);
        }
    }
}
