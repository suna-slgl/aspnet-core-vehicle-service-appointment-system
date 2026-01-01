using System.ComponentModel.DataAnnotations;

namespace VehicleServiceApp.Validation
{
    /// <summary>
    /// Custom Validation Attribute - Validates that a date is in the future
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FutureDateAttribute : ValidationAttribute
    {
        public int MinDaysInFuture { get; set; } = 0;
        public int MaxDaysInFuture { get; set; } = 365;

        public FutureDateAttribute()
        {
            ErrorMessage = "Tarih bugünden sonra olmalıdır.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateTime dateValue)
            {
                var today = DateTime.Today;
                var minDate = today.AddDays(MinDaysInFuture);
                var maxDate = today.AddDays(MaxDaysInFuture);

                if (dateValue.Date < minDate)
                {
                    return new ValidationResult(
                        ErrorMessage ?? $"Tarih en az {minDate:dd.MM.yyyy} tarihinde olmalıdır.",
                        new[] { validationContext.MemberName! });
                }

                if (dateValue.Date > maxDate)
                {
                    return new ValidationResult(
                        $"Tarih en fazla {maxDate:dd.MM.yyyy} tarihinde olabilir.",
                        new[] { validationContext.MemberName! });
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Geçersiz tarih formatı.");
        }
    }

    /// <summary>
    /// Custom Validation Attribute - Validates Turkish license plate format
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class TurkishLicensePlateAttribute : ValidationAttribute
    {
        public TurkishLicensePlateAttribute()
        {
            ErrorMessage = "Geçerli bir Türkiye plakası giriniz (Örn: 34 ABC 123)";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var plateNumber = value.ToString()!.ToUpper().Replace(" ", "");

            // Turkish license plate patterns:
            // XX YYY NNN (City code + letters + numbers)
            // XX YY NNNN
            // XX Y NNNNN
            var patterns = new[]
            {
                @"^(0[1-9]|[1-7][0-9]|8[01])[A-Z]{1,3}[0-9]{2,4}$"
            };

            foreach (var pattern in patterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(plateNumber, pattern))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
        }
    }

    /// <summary>
    /// Custom Validation Attribute - Validates working hours
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class WorkingHoursAttribute : ValidationAttribute
    {
        public int StartHour { get; set; } = 8;
        public int EndHour { get; set; } = 18;

        public WorkingHoursAttribute()
        {
            ErrorMessage = "Seçilen saat çalışma saatleri dışındadır.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            TimeSpan time;

            if (value is TimeSpan timeValue)
            {
                time = timeValue;
            }
            else if (value is string stringValue && TimeSpan.TryParse(stringValue, out var parsedTime))
            {
                time = parsedTime;
            }
            else
            {
                return new ValidationResult("Geçersiz saat formatı.");
            }

            var startTime = new TimeSpan(StartHour, 0, 0);
            var endTime = new TimeSpan(EndHour, 0, 0);

            if (time < startTime || time >= endTime)
            {
                return new ValidationResult(
                    ErrorMessage ?? $"Randevu saati {StartHour:00}:00 - {EndHour:00}:00 arasında olmalıdır.",
                    new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Custom Validation Attribute - Validates file extension
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AllowedFileExtensionsAttribute : ValidationAttribute
    {
        public string[] Extensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif" };

        public AllowedFileExtensionsAttribute()
        {
            ErrorMessage = "Geçersiz dosya formatı.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is Microsoft.AspNetCore.Http.IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!Extensions.Contains(extension))
                {
                    return new ValidationResult(
                        ErrorMessage ?? $"İzin verilen formatlar: {string.Join(", ", Extensions)}",
                        new[] { validationContext.MemberName! });
                }
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Custom Validation Attribute - Validates maximum file size
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        public int MaxSizeInMB { get; set; } = 5;

        public MaxFileSizeAttribute()
        {
            ErrorMessage = "Dosya boyutu çok büyük.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is Microsoft.AspNetCore.Http.IFormFile file)
            {
                var maxSizeInBytes = MaxSizeInMB * 1024 * 1024;
                if (file.Length > maxSizeInBytes)
                {
                    return new ValidationResult(
                        ErrorMessage ?? $"Dosya boyutu en fazla {MaxSizeInMB} MB olabilir.",
                        new[] { validationContext.MemberName! });
                }
            }

            return ValidationResult.Success;
        }
    }
}
