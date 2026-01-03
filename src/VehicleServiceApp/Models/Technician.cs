using System.ComponentModel.DataAnnotations;

namespace VehicleServiceApp.Models
{
    /// <summary>
    /// Technician Entity - Represents service technicians
    /// </summary>
    public class Technician
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Telefon")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [StringLength(100)]
        [Display(Name = "Uzmanlık Alanı")]
        public string? Specialization { get; set; }

        [Display(Name = "Deneyim (Yıl)")]
        [Range(0, 50, ErrorMessage = "Geçerli bir deneyim yılı giriniz")]
        public int? ExperienceYears { get; set; }

        [Display(Name = "Fotoğraf")]
        public string? ImagePath { get; set; }

        [Display(Name = "Çalışma Başlangıç Saati")]
        public TimeSpan WorkStartTime { get; set; } = new TimeSpan(8, 0, 0);

        [Display(Name = "Çalışma Bitiş Saati")]
        public TimeSpan WorkEndTime { get; set; } = new TimeSpan(18, 0, 0);

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Computed Properties
        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Çalışma Saatleri")]
        public string WorkingHours => $"{WorkStartTime:hh\\:mm} - {WorkEndTime:hh\\:mm}";
    }
}

