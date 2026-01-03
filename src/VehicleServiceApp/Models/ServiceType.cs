using System.ComponentModel.DataAnnotations;

namespace VehicleServiceApp.Models
{
    /// <summary>
    /// Service Type Entity - Represents different types of vehicle services
    /// </summary>
    public class ServiceType
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Servis adı zorunludur")]
        [StringLength(100, ErrorMessage = "Servis adı en fazla 100 karakter olabilir")]
        [Display(Name = "Servis Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Tahmini süre zorunludur")]
        [Range(15, 480, ErrorMessage = "Süre 15-480 dakika arasında olmalıdır")]
        [Display(Name = "Tahmini Süre (Dakika)")]
        public int EstimatedDurationMinutes { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0, 100000, ErrorMessage = "Geçerli bir fiyat giriniz")]
        [Display(Name = "Fiyat (₺)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "İkon")]
        public string? IconClass { get; set; }

        [Display(Name = "Renk Kodu")]
        public string? ColorCode { get; set; }

        [Display(Name = "Sıralama")]
        public int SortOrder { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Computed Property
        [Display(Name = "Süre (Saat)")]
        public string FormattedDuration
        {
            get
            {
                var hours = EstimatedDurationMinutes / 60;
                var minutes = EstimatedDurationMinutes % 60;
                if (hours > 0 && minutes > 0)
                    return $"{hours} saat {minutes} dk";
                else if (hours > 0)
                    return $"{hours} saat";
                else
                    return $"{minutes} dk";
            }
        }
    }
}

