using System.ComponentModel.DataAnnotations;
using VehicleServiceApp.Validation;
using Xunit;

namespace VehicleServiceApp.Tests;

public class FutureDateAttributeTests
{
    [Fact]
    public void IsValid_ReturnsValidationError_ForPastDate()
    {
        var attribute = new FutureDateAttribute();
        var context = new ValidationContext(new object()) { MemberName = "AppointmentDate" };

        var result = attribute.GetValidationResult(DateTime.Today.AddDays(-1), context);

        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_ReturnsSuccess_ForFutureDate()
    {
        var attribute = new FutureDateAttribute();
        var context = new ValidationContext(new object()) { MemberName = "AppointmentDate" };

        var result = attribute.GetValidationResult(DateTime.Today.AddDays(1), context);

        Assert.Equal(ValidationResult.Success, result);
    }
}
