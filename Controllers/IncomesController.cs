using Drivious.DTOs.Income;
using Drivious.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Drivious.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IncomesController : ControllerBase
    {
        private readonly IIncomeService _service;

        public IncomesController(IIncomeService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(IncomeCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);

            return result.Success ? StatusCode(StatusCodes.Status201Created, result) : BadRequest(result);
        }

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

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, IncomeUpdateDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            var result = await _service.ToggleAsync(id);

            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
