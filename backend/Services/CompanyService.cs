
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CompanyService
{
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto?> GetByIdAsync(int id, int companyId)
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(company =>
                company.Id == id &&
                company.Id == companyId)
            .Select(company => new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                TaxId = company.TaxId,
                CreatedAt = company.CreatedAt,
                IsActive = company.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CompanyDto?> UpdateAsync(
        int id,
        UpdateCompanyDto dto,
        int companyId)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(company =>
                company.Id == id &&
                company.Id == companyId);

        if (company is null)
        {
            return null;
        }

        company.Name = dto.Name;
        company.TaxId = dto.TaxId;
        company.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            TaxId = company.TaxId,
            CreatedAt = company.CreatedAt,
            IsActive = company.IsActive
        };
    }
}
