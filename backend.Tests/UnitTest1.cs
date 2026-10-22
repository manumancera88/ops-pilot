
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Tests;

public class CustomerServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"OpsPilotTests_{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);

        context.Companies.AddRange(
            new Company { Id = 1, Name = "Empresa A" },
            new Company { Id = 2, Name = "Empresa B" }
        );

        context.Customers.AddRange(
            new Customer
            {
                Id = 1,
                Name = "Cliente A",
                CompanyId = 1
            },
            new Customer
            {
                Id = 2,
                Name = "Cliente B",
                CompanyId = 2
            }
        );

        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCustomersFromRequestedCompany()
    {
        using var context = CreateContext();
        var service = new CustomerService(context);

        var result = await service.GetAllAsync(1);

        var customer = Assert.Single(result);
        Assert.Equal("Cliente A", customer.Name);
        Assert.Equal(1, customer.CompanyId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenCustomerBelongsToAnotherCompany()
    {
        using var context = CreateContext();
        var service = new CustomerService(context);

        var result = await service.GetByIdAsync(2, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_UsesCompanyIdProvidedByService()
    {
        using var context = CreateContext();
        var service = new CustomerService(context);

        var dto = new CreateCustomerDto
        {
            Name = "Novo cliente",
            CompanyId = 2
        };

        var result = await service.CreateAsync(dto, 1);

        Assert.NotNull(result);
        Assert.Equal(1, result.CompanyId);

        var savedCustomer = await context.Customers
            .SingleAsync(customer => customer.Name == "Novo cliente");

        Assert.Equal(1, savedCustomer.CompanyId);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNullWhenCompanyDoesNotExist()
    {
        using var context = CreateContext();
        var service = new CustomerService(context);

        var dto = new CreateCustomerDto
        {
            Name = "Cliente sem empresa"
        };

        var result = await service.CreateAsync(dto, 999);

        Assert.Null(result);
        Assert.False(
            await context.Customers.AnyAsync(
                customer => customer.Name == "Cliente sem empresa"));
    }

    [Fact]
    public async Task UpdateAsync_DoesNotUpdateCustomerFromAnotherCompany()
    {
        using var context = CreateContext();
        var service = new CustomerService(context);

        var dto = new UpdateCustomerDto
        {
            Name = "Nome alterado",
            IsActive = true
        };

        var result = await service.UpdateAsync(2, dto, 1);

        Assert.Null(result);

        var savedCustomer = await context.Customers.FindAsync(2);

        Assert.NotNull(savedCustomer);
        Assert.Equal("Cliente B", savedCustomer.Name);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotDeleteCustomerFromAnotherCompany()
    {
        using var context = CreateContext();
        var service = new CustomerService(context);

        var result = await service.DeleteAsync(2, 1);

        Assert.False(result);
        Assert.True(
            await context.Customers.AnyAsync(customer => customer.Id == 2));
    }
}
