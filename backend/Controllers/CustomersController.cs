
using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _service;

    public CustomersController(CustomerService service)
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
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var customers = await _service.GetAllAsync(companyId.Value);

        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var customer = await _service.GetByIdAsync(
            id, companyId.Value);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(
        CreateCustomerDto dto)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var customer = await _service.CreateAsync(
            dto, companyId.Value);

        if (customer is null)
            return BadRequest("The authenticated company does not exist.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = customer.Id },
            customer);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerDto>> Update(
        int id, UpdateCustomerDto dto)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var customer = await _service.UpdateAsync(
            id, dto, companyId.Value);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var deleted = await _service.DeleteAsync(
            id, companyId.Value);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
