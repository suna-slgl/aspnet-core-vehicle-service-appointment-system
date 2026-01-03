using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VehicleServiceApp.Models
{
    /// <summary>
    /// Custom Identity User - Extends IdentityUser with additional properties
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Profil Fotoğrafı")]
        public string? ProfileImagePath { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Son Güncelleme")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Computed Property
        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";
    }
}

