using MedTime.Models.Requests;
using MedTime.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedTime.Controllers
{
    [ApiController]
    [Route("api/appointment")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _service;
        public AppointmentController(AppointmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] AppointmentCreate request)
        {
            var createdDto = await _service.CreateAsync(request);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = createdDto.Appointmentid }, createdDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] AppointmentUpdate request)
        {
            var result = await _service.UpdateAsync(id, request);
            if (result == null) return NotFound();
            if (result == false) return BadRequest();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (result == null) return NotFound();
            if (result == false) return BadRequest();
            return NoContent();
        }
    }
}
