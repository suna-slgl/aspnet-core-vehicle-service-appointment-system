using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehicleServiceApp.Validation;

namespace VehicleServiceApp.Models
{
    /// <summary>
    /// Appointment Entity - Represents service appointments
    /// </summary>
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Randevu tarihi bugünden sonra olmalıdır")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Randevu saati zorunludur")]
        [Display(Name = "Randevu Saati")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        [Display(Name = "Randevu Durumu")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Müşteri Notu")]
        public string? CustomerNotes { get; set; }

        [StringLength(500)]
        [Display(Name = "Teknisyen Notu")]
        public string? TechnicianNotes { get; set; }

        [StringLength(500)]
        [Display(Name = "İptal Sebebi")]
        public string? CancellationReason { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Onay Tarihi")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "Tamamlanma Tarihi")]
        public DateTime? CompletedAt { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Kullanıcı seçimi zorunludur")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Araç seçimi zorunludur")]
        [Display(Name = "Araç")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Servis türü seçimi zorunludur")]
        [Display(Name = "Servis Türü")]
        public int ServiceTypeId { get; set; }

        [Display(Name = "Teknisyen")]
        public int? TechnicianId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }

        [ForeignKey("ServiceTypeId")]
        public virtual ServiceType? ServiceType { get; set; }

        [ForeignKey("TechnicianId")]
        public virtual Technician? Technician { get; set; }

        // Computed Properties
        [Display(Name = "Randevu Tarihi ve Saati")]
        public DateTime FullDateTime => AppointmentDate.Date.Add(AppointmentTime);

        [Display(Name = "Durum Rengi")]
        public string StatusColor => Status switch
        {
            AppointmentStatus.Pending => "warning",
            AppointmentStatus.Confirmed => "primary",
            AppointmentStatus.InProgress => "info",
            AppointmentStatus.Completed => "success",
            AppointmentStatus.Cancelled => "danger",
            _ => "secondary"
        };

        [Display(Name = "Durum İkonu")]
        public string StatusIcon => Status switch
        {
            AppointmentStatus.Pending => "bi-clock",
            AppointmentStatus.Confirmed => "bi-check-circle",
            AppointmentStatus.InProgress => "bi-gear",
            AppointmentStatus.Completed => "bi-check-all",
            AppointmentStatus.Cancelled => "bi-x-circle",
            _ => "bi-question-circle"
        };
    }

    /// <summary>
    /// Appointment Status Enum
    /// </summary>
    public enum AppointmentStatus
    {
        [Display(Name = "Beklemede")]
        Pending = 0,

        [Display(Name = "Onaylandı")]
        Confirmed = 1,

        [Display(Name = "Devam Ediyor")]
        InProgress = 2,

        [Display(Name = "Tamamlandı")]
        Completed = 3,

        [Display(Name = "İptal Edildi")]
        Cancelled = 4
    }
}
