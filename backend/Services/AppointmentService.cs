using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AppointmentService
{
    private readonly AppDbContext _context;

    public AppointmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AppointmentDto>> GetAllAsync()
    {
        return await _context.Appointments
            .AsNoTracking()
            .Select(appointment => new AppointmentDto
            {
                Id = appointment.Id,
                Title = appointment.Title,
                StartAt = appointment.StartAt,
                EndAt = appointment.EndAt,
                Notes = appointment.Notes,
                IsCancelled = appointment.IsCancelled,
                CompanyId = appointment.CompanyId,
                CustomerId = appointment.CustomerId
            })
            .ToListAsync();
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.Id == id)
            .Select(appointment => new AppointmentDto
            {
                Id = appointment.Id,
                Title = appointment.Title,
                StartAt = appointment.StartAt,
                EndAt = appointment.EndAt,
                Notes = appointment.Notes,
                IsCancelled = appointment.IsCancelled,
                CompanyId = appointment.CompanyId,
                CustomerId = appointment.CustomerId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(AppointmentDto? Appointment, string? Error)> CreateAsync(
        CreateAppointmentDto dto)
    {
        if (dto.EndAt <= dto.StartAt)
        {
            return (null, "EndAt must be later than StartAt.");
        }

        var companyExists = await _context.Companies
            .AnyAsync(company => company.Id == dto.CompanyId);

        if (!companyExists)
        {
            return (null, "The specified company does not exist.");
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(customer =>
                customer.Id == dto.CustomerId);

        if (customer is null)
        {
            return (null, "The specified customer does not exist.");
        }

        if (customer.CompanyId != dto.CompanyId)
        {
            return (null, "The customer does not belong to the specified company.");
        }

        var appointment = new Appointment
        {
            Title = dto.Title,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            Notes = dto.Notes,
            CompanyId = dto.CompanyId,
            CustomerId = dto.CustomerId
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return (
            new AppointmentDto
            {
                Id = appointment.Id,
                Title = appointment.Title,
                StartAt = appointment.StartAt,
                EndAt = appointment.EndAt,
                Notes = appointment.Notes,
                IsCancelled = appointment.IsCancelled,
                CompanyId = appointment.CompanyId,
                CustomerId = appointment.CustomerId
            },
            null
        );
    }

    public async Task<(AppointmentDto? Appointment, string? Error)> UpdateAsync(
        int id,
        UpdateAppointmentDto dto)
    {
        if (dto.EndAt <= dto.StartAt)
        {
            return (null, "EndAt must be later than StartAt.");
        }

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(appointment => appointment.Id == id);

        if (appointment is null)
        {
            return (null, "Appointment not found.");
        }

        appointment.Title = dto.Title;
        appointment.StartAt = dto.StartAt;
        appointment.EndAt = dto.EndAt;
        appointment.Notes = dto.Notes;
        appointment.IsCancelled = dto.IsCancelled;

        await _context.SaveChangesAsync();

        return (
            new AppointmentDto
            {
                Id = appointment.Id,
                Title = appointment.Title,
                StartAt = appointment.StartAt,
                EndAt = appointment.EndAt,
                Notes = appointment.Notes,
                IsCancelled = appointment.IsCancelled,
                CompanyId = appointment.CompanyId,
                CustomerId = appointment.CustomerId
            },
            null
        );
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(appointment => appointment.Id == id);

        if (appointment is null)
        {
            return false;
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return true;
    }
}