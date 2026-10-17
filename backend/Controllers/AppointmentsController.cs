
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _service;

    public AppointmentsController(AppointmentService service)
    {
        _service = service;
    }

    private int? GetCompanyId()
    {
        var value = User.FindFirst("companyId")?.Value;

        return int.TryParse(value, out var companyId)
            ? companyId
            : null;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppointmentDto>>> GetAll()
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var appointments = await _service.GetAllAsync(companyId.Value);

        return Ok(appointments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var appointment = await _service.GetByIdAsync(id, companyId.Value);

        if (appointment is null)
            return NotFound();

        return Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        CreateAppointmentDto dto)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var result = await _service.CreateAsync(dto, companyId.Value);

        if (result.Appointment is null)
            return BadRequest(result.Error);

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
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var result = await _service.UpdateAsync(
            id,
            dto,
            companyId.Value);

        if (result.Appointment is null)
        {
            if (result.Error == "Appointment not found.")
                return NotFound();

            return BadRequest(result.Error);
        }

        return Ok(result.Appointment);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var deleted = await _service.DeleteAsync(id, companyId.Value);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
