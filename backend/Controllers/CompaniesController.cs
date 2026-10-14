using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly CompanyService _service;

    public CompaniesController(CompanyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyDto>>> GetAll()
    {
        var companies = await _service.GetAllAsync();

        return Ok(companies);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyDto>> GetById(int id)
    {
        var company = await _service.GetByIdAsync(id);

        if (company is null)
        {
            return NotFound();
        }

        return Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(CreateCompanyDto dto)
    {
        var company = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = company.Id },
            company);
    }
    
    [HttpPut("{id:int}")]
public async Task<ActionResult<CompanyDto>> Update(
    int id,
    UpdateCompanyDto dto)
{
    var company = await _service.UpdateAsync(id, dto);

    if (company is null)
    {
        return NotFound();
    }

    return Ok(company);
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