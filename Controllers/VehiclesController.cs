using Drivious.DTOs.Vehicle;
using Drivious.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Drivious.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _service;

        public VehiclesController(IVehicleService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] VehicleCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);

            return result ? StatusCode(201, "Created successful!") : BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            var result = await _service.RemoveAsync(id);

            return result ? StatusCode(204) : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetAsync(id);

            return Ok(result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] VehicleUpdateDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            return result ? Ok("Updated successful!") : BadRequest();
        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            var result = await _service.ToggleAsync(id);

            return result ? Ok("Status changed successfully!") : BadRequest();
        }
    }
}

