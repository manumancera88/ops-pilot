
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetAllAsync(int companyId)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(customer => customer.CompanyId == companyId)
            .Select(customer => new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                CreatedAt = customer.CreatedAt,
                IsActive = customer.IsActive,
                CompanyId = customer.CompanyId
            })
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(
        int id, int companyId)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(customer =>
                customer.Id == id &&
                customer.CompanyId == companyId)
            .Select(customer => new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                CreatedAt = customer.CreatedAt,
                IsActive = customer.IsActive,
                CompanyId = customer.CompanyId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerDto?> CreateAsync(
        CreateCustomerDto dto, int companyId)
    {
        var companyExists = await _context.Companies
            .AnyAsync(company => company.Id == companyId);

        if (!companyExists)
        {
            return null;
        }

        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            CompanyId = companyId
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            CreatedAt = customer.CreatedAt,
            IsActive = customer.IsActive,
            CompanyId = customer.CompanyId
        };
    }

    public async Task<CustomerDto?> UpdateAsync(
        int id, UpdateCustomerDto dto, int companyId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(customer =>
                customer.Id == id &&
                customer.CompanyId == companyId);

        if (customer is null)
        {
            return null;
        }

        customer.Name = dto.Name;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            CreatedAt = customer.CreatedAt,
            IsActive = customer.IsActive,
            CompanyId = customer.CompanyId
        };
    }

    public async Task<bool> DeleteAsync(int id, int companyId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(customer =>
                customer.Id == id &&
                customer.CompanyId == companyId);

        if (customer is null)
        {
            return false;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return true;
    }
}
