using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleServiceApp.Models
{
    /// <summary>
    /// Vehicle Entity - Represents a vehicle owned by a user
    /// </summary>
    public class Vehicle
    {
        [Key]
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
        public int Year { get; set; }

        [StringLength(20)]
        [Display(Name = "Renk")]
        public string? Color { get; set; }

        [Display(Name = "Kilometre")]
        [Range(0, 2000000, ErrorMessage = "Geçerli bir kilometre değeri giriniz")]
        public int? Mileage { get; set; }

        [Display(Name = "Yakıt Türü")]
        public FuelType FuelType { get; set; } = FuelType.Gasoline;

        [Display(Name = "Araç Fotoğrafı")]
        public string? ImagePath { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Computed Property
        [Display(Name = "Araç Bilgisi")]
        public string VehicleInfo => $"{Brand} {Model} ({Year}) - {LicensePlate}";
    }

    /// <summary>
    /// Fuel Type Enum
    /// </summary>
    public enum FuelType
    {
        [Display(Name = "Benzin")]
        Gasoline = 0,

        [Display(Name = "Dizel")]
        Diesel = 1,

        [Display(Name = "LPG")]
        LPG = 2,

        [Display(Name = "Elektrik")]
        Electric = 3,

        [Display(Name = "Hibrit")]
        Hybrid = 4
    }
}
