using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Data;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services;
using Xunit;

namespace VehicleServiceApp.Tests;

public class VehicleServiceTests
{
    [Fact]
    public async Task IsLicensePlateExistsAsync_NormalizesWhitespaceAndCasing()
    {
        await using var context = CreateContext();
        context.Vehicles.Add(new Vehicle
        {
            Id = 1,
            UserId = "user-1",
            LicensePlate = "34 ABC 123",
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2024,
            IsActive = true
        });
        await context.SaveChangesAsync();
        var service = new VehicleService(context);

        var exists = await service.IsLicensePlateExistsAsync("34abc123");

        Assert.True(exists);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
