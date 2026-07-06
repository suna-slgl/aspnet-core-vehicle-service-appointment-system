using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Data;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services;
using Xunit;

namespace VehicleServiceApp.Tests;

public class AppointmentServiceTests
{
    [Fact]
    public async Task IsTimeSlotAvailableAsync_ReturnsFalse_WhenSlotCapacityIsFull()
    {
        await using var context = CreateContext();
        for (var i = 0; i < 5; i++)
        {
            context.Appointments.Add(CreateAppointment(i + 1, technicianId: null));
        }

        await context.SaveChangesAsync();
        var service = new AppointmentService(context);

        var isAvailable = await service.IsTimeSlotAvailableAsync(DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0));

        Assert.False(isAvailable);
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_ReturnsFalse_WhenTechnicianAlreadyAssigned()
    {
        await using var context = CreateContext();
        context.Appointments.Add(CreateAppointment(1, technicianId: 7));
        await context.SaveChangesAsync();
        var service = new AppointmentService(context);

        var isAvailable = await service.IsTimeSlotAvailableAsync(DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0), 7);

        Assert.False(isAvailable);
    }

    [Fact]
    public async Task DeleteAppointmentAsync_SoftDeletesAppointment()
    {
        await using var context = CreateContext();
        context.Appointments.Add(CreateAppointment(1, technicianId: null));
        await context.SaveChangesAsync();
        var service = new AppointmentService(context);

        var deleted = await service.DeleteAppointmentAsync(1);
        var appointment = await context.Appointments.IgnoreQueryFilters().FirstAsync(a => a.Id == 1);

        Assert.True(deleted);
        Assert.False(appointment.IsActive);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Appointment CreateAppointment(int id, int? technicianId)
    {
        return new Appointment
        {
            Id = id,
            UserId = "user-1",
            VehicleId = id,
            ServiceTypeId = 1,
            TechnicianId = technicianId,
            AppointmentDate = DateTime.Today.AddDays(1),
            AppointmentTime = new TimeSpan(9, 0, 0),
            Status = AppointmentStatus.Pending,
            IsActive = true
        };
    }
}
