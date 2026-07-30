using Drivious.DTOs.Vehicle;
using Drivious.Constants;
using Drivious.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Drivious.Controllers
{
    [Authorize(Roles = AppRoles.ReadFleet)]
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _service;

        public VehiclesController(IVehicleService service)
        {
            _service = service;
        }

        [Authorize(Roles = AppRoles.ManageFleet)]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] VehicleCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);

            return result.Success ? StatusCode(StatusCodes.Status201Created, result) : BadRequest(result);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            var result = await _service.RemoveAsync(id);

            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        [Authorize(Roles = AppRoles.ManageFleet)]
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _service.GetDeletedAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetAsync(id);

            return result.Success ? Ok(result) : NotFound(result);
        }

        [Authorize(Roles = AppRoles.ManageFleet)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] VehicleUpdateDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = AppRoles.ManageFleet)]
        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            var result = await _service.ToggleAsync(id);

            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}

