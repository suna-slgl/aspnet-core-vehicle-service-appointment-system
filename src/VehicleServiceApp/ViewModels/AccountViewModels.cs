using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VehicleServiceApp.ViewModels
{
    /// <summary>
    /// ViewModel for user registration
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrarı")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Profil Fotoğrafı")]
        public IFormFile? ProfileImage { get; set; }
    }

    /// <summary>
    /// ViewModel for user login
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    /// <summary>
    /// ViewModel for user profile
    /// </summary>
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public IFormFile? ProfileImage { get; set; }

        public string? ExistingProfileImagePath { get; set; }
        
        public string? ProfileImagePath { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; }

        // Statistics
        public int TotalVehicles { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
    }

    /// <summary>
    /// ViewModel for changing password
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre zorunludur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre tekrarı zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrarı")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for admin user management
    /// </summary>
    public class UserManageViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public int VehicleCount { get; set; }
        public int AppointmentCount { get; set; }
    }
}
