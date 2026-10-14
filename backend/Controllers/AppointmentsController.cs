using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _service;

    public AppointmentsController(AppointmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppointmentDto>>> GetAll()
    {
        var appointments = await _service.GetAllAsync();

        return Ok(appointments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        var appointment = await _service.GetByIdAsync(id);

        if (appointment is null)
        {
            return NotFound();
        }

        return Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        CreateAppointmentDto dto)
    {
        var result = await _service.CreateAsync(dto);

        if (result.Appointment is null)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Appointment.Id },
            result.Appointment);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> Update(
        int id,
        UpdateAppointmentDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        if (result.Appointment is null)
        {
            if (result.Error == "Appointment not found.")
            {
                return NotFound();
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Appointment);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}