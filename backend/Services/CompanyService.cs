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

    public async Task<List<CompanyDto>> GetAllAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .Select(company => new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                TaxId = company.TaxId,
                CreatedAt = company.CreatedAt,
                IsActive = company.IsActive
            })
            .ToListAsync();
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(company => company.Id == id)
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

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto)
    {
        var company = new Company
        {
            Name = dto.Name,
            TaxId = dto.TaxId
        };

        _context.Companies.Add(company);

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

    public async Task<CompanyDto?> UpdateAsync(int id, UpdateCompanyDto dto)
{
    var company = await _context.Companies.FindAsync(id);

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

    public async Task<bool> DeleteAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company is null)
        {
            return false;
        }

        _context.Companies.Remove(company);

        await _context.SaveChangesAsync();

        return true;
    }
}