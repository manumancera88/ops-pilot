
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
            .UseInMemoryDatabase($"AppointmentTests_{Guid.NewGuid()}")
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

        context.Appointments.AddRange(
            new Appointment
            {
                Id = 1,
                Title = "Cita Empresa A",
                StartAt = new DateTime(2026, 10, 23, 10, 0, 0),
                EndAt = new DateTime(2026, 10, 23, 11, 0, 0),
                CompanyId = 1,
                CustomerId = 1
            },
            new Appointment
            {
                Id = 2,
                Title = "Cita Empresa B",
                StartAt = new DateTime(2026, 10, 23, 14, 0, 0),
                EndAt = new DateTime(2026, 10, 23, 15, 0, 0),
                CompanyId = 2,
                CustomerId = 2
            }
        );

        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyAppointmentsFromRequestedCompany()
    {
        using var context = CreateContext();
        var service = new AppointmentService(context);

        var result = await service.GetAllAsync(1);

        var appointment = Assert.Single(result);
        Assert.Equal("Cita Empresa A", appointment.Title);
        Assert.Equal(1, appointment.CompanyId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForAnotherCompanyAppointment()
    {
        using var context = CreateContext();
        var service = new AppointmentService(context);

        var result = await service.GetByIdAsync(2, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesAppointmentForAuthenticatedCompany()
    {
        using var context = CreateContext();
        var service = new AppointmentService(context);

        var dto = new CreateAppointmentDto
        {
            Title = "Nueva cita",
            StartAt = new DateTime(2026, 10, 24, 10, 0, 0),
            EndAt = new DateTime(2026, 10, 24, 11, 0, 0),
            CompanyId = 2,
            CustomerId = 1
        };

        var (appointment, error) = await service.CreateAsync(dto, 1);

        Assert.Null(error);
        Assert.NotNull(appointment);
        Assert.Equal(1, appointment.CompanyId);
        Assert.Equal(1, appointment.CustomerId);

        var saved = await context.Appointments
            .SingleAsync(item => item.Title == "Nueva cita");

        Assert.Equal(1, saved.CompanyId);
    }

    [Fact]
    public async Task CreateAsync_RejectsCustomerFromAnotherCompany()
    {
        using var context = CreateContext();
        var service = new AppointmentService(context);

        var dto = new CreateAppointmentDto
        {
            Title = "Cita no permitida",
            StartAt = new DateTime(2026, 10, 24, 10, 0, 0),
            EndAt = new DateTime(2026, 10, 24, 11, 0, 0),
            CustomerId = 2
        };

        var (appointment, error) = await service.CreateAsync(dto, 1);

        Assert.Null(appointment);
        Assert.Equal(
            "The specified customer does not exist in your company.",
            error
        );

        Assert.False(await context.Appointments
            .AnyAsync(item => item.Title == "Cita no permitida"));
    }

    [Fact]
    public async Task CreateAsync_RejectsEndAtEarlierThanStartAt()
    {
        using var context = CreateContext();
        var service = new AppointmentService(context);

        var dto = new CreateAppointmentDto
        {
            Title = "Cita con fechas incorrectas",
            StartAt = new DateTime(2026, 10, 24, 11, 0, 0),
            EndAt = new DateTime(2026, 10, 24, 10, 0, 0),
            CustomerId = 1
        };

        var (appointment, error) = await service.CreateAsync(dto, 1);

        Assert.Null(appointment);
        Assert.Equal("EndAt must be later than StartAt.", error);
    }

    [Fact]
    public async Task CreateAsync_RejectsEndAtEqualToStartAt()
    {
        using var context = CreateContext();
        var service = new AppointmentService(context);

        var sameTime = new DateTime(2026, 10, 24, 10, 0, 0);

        var dto = new CreateAppointmentDto
        {
            Title = "Cita sin duración",
            StartAt = sameTime,
            EndAt = sameTime,
            CustomerId = 1
        };

        var (appointment, error) = await service.CreateAsync(dto, 1);

        Assert.Null(appointment);
        Assert.Equal("EndAt must be later than StartAt.", error);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotUpdateAnotherCompanyAppointment()
    {
        using var context = CreateContext();
        var service = new AppointmentService(context);

        var dto = new UpdateAppointmentDto
        {
            Title = "Título modificado",
            StartAt = new DateTime(2026, 10, 23, 14, 0, 0),
            EndAt = new DateTime(2026, 10, 23, 15, 0, 0),
            IsCancelled = true
        };

        var (appointment, error) = await service.UpdateAsync(2, dto, 1);

        Assert.Null(appointment);
        Assert.Equal("Appointment not found.", error);

        var saved = await context.Appointments.FindAsync(2);

        Assert.NotNull(saved);
        Assert.Equal("Cita Empresa B", saved.Title);
        Assert.False(saved.IsCancelled);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotDeleteAnotherCompanyAppointment()
    {
        using var context = CreateContext();
        var service = new AppointmentService(context);

        var result = await service.DeleteAsync(2, 1);

        Assert.False(result);
        Assert.True(await context.Appointments.AnyAsync(item => item.Id == 2));
    }
}
