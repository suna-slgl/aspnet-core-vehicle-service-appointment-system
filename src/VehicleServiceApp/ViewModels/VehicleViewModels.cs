using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using VehicleServiceApp.Models;

namespace VehicleServiceApp.ViewModels
{
    /// <summary>
    /// ViewModel for creating/editing vehicles
    /// </summary>
    public class VehicleCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Plaka alanı zorunludur")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Plaka 5-15 karakter arasında olmalıdır")]
        [Display(Name = "Plaka")]
        [RegularExpression(@"^[0-9]{2}\s?[A-Z]{1,3}\s?[0-9]{2,4}$", ErrorMessage = "Geçerli bir plaka giriniz (Örn: 34 ABC 123)")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Marka alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Marka en fazla 50 karakter olabilir")]
        [Display(Name = "Marka")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Model en fazla 50 karakter olabilir")]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yıl alanı zorunludur")]
        [Range(1900, 2030, ErrorMessage = "Geçerli bir yıl giriniz")]
        [Display(Name = "Model Yılı")]
        public int Year { get; set; } = DateTime.Now.Year;

        [StringLength(20)]
        [Display(Name = "Renk")]
        public string? Color { get; set; }

        [Display(Name = "Kilometre")]
        [Range(0, 2000000, ErrorMessage = "Geçerli bir kilometre değeri giriniz")]
        public int? Mileage { get; set; }

        [Display(Name = "Yakıt Türü")]
        public FuelType FuelType { get; set; } = FuelType.Gasoline;

        [Display(Name = "Araç Fotoğrafı")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImagePath { get; set; }

        [StringLength(500)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel for editing vehicles - same as create but with Id
    /// </summary>
    public class VehicleEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Plaka alanı zorunludur")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Plaka 5-15 karakter arasında olmalıdır")]
        [Display(Name = "Plaka")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Marka alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Marka en fazla 50 karakter olabilir")]
        [Display(Name = "Marka")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Model en fazla 50 karakter olabilir")]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yıl alanı zorunludur")]
        [Range(1900, 2030, ErrorMessage = "Geçerli bir yıl giriniz")]
        [Display(Name = "Model Yılı")]
        public int Year { get; set; }

        [StringLength(20)]
        [Display(Name = "Renk")]
        public string? Color { get; set; }

        [Display(Name = "Kilometre")]
        [Range(0, 2000000, ErrorMessage = "Geçerli bir kilometre değeri giriniz")]
        public int? Mileage { get; set; }

        [Display(Name = "Yakıt Türü")]
        public FuelType FuelType { get; set; }
    }

    /// <summary>
    /// ViewModel for displaying vehicle details
    /// </summary>
    public class VehicleDetailViewModel
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? Color { get; set; }
        public int? Mileage { get; set; }
        public FuelType FuelType { get; set; }
        public string? ImagePath { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Computed
        public string VehicleInfo => $"{Brand} {Model} ({Year})";
        public string FuelTypeDisplay => FuelType.ToString();

        // Related data
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public List<AppointmentDetailViewModel> RecentAppointments { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for vehicle list
    /// </summary>
    public class VehicleListViewModel
    {
        public List<VehicleDetailViewModel> Vehicles { get; set; } = new();
        public int TotalCount { get; set; }
        public string? SearchTerm { get; set; }
    }
}
