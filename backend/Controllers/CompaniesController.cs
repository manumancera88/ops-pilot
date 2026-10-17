
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    private int? GetCompanyId()
    {
        var value = User.FindFirst("companyId")?.Value;

        return int.TryParse(value, out var companyId)
            ? companyId
            : null;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyDto>> GetById(int id)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var company = await _service.GetByIdAsync(
            id,
            companyId.Value);

        if (company is null)
            return NotFound();

        return Ok(company);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CompanyDto>> Update(
        int id,
        UpdateCompanyDto dto)
    {
        var companyId = GetCompanyId();

        if (companyId is null)
            return Unauthorized();

        var company = await _service.UpdateAsync(
            id,
            dto,
            companyId.Value);

        if (company is null)
            return NotFound();

        return Ok(company);
    }
}
