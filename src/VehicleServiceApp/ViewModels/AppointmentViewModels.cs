using System.ComponentModel.DataAnnotations;
using VehicleServiceApp.Models;
using VehicleServiceApp.Validation;

namespace VehicleServiceApp.ViewModels
{
    /// <summary>
    /// ViewModel for creating/editing appointments
    /// </summary>
    public class AppointmentCreateViewModel
    {
        [Required(ErrorMessage = "Araç seçimi zorunludur")]
        [Display(Name = "Araç")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Servis türü seçimi zorunludur")]
        [Display(Name = "Servis Türü")]
        public int ServiceTypeId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Randevu tarihi bugünden sonra olmalıdır")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Randevu saati zorunludur")]
        [Display(Name = "Randevu Saati")]
        public string AppointmentTime { get; set; } = "09:00";

        [StringLength(500)]
        [Display(Name = "Notlarınız")]
        public string? CustomerNotes { get; set; }

        // For dropdown lists
        public List<Vehicle> AvailableVehicles { get; set; } = new();
        public List<ServiceType> AvailableServiceTypes { get; set; } = new();
        public List<Technician> AvailableTechnicians { get; set; } = new();
        public List<string> AvailableTimeSlots { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for displaying appointment details
    /// </summary>
    public class AppointmentDetailViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? CustomerNotes { get; set; }
        public string? TechnicianNotes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Related entities info
        public string VehicleInfo { get; set; } = string.Empty;
        public string ServiceTypeName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public string ServiceDuration { get; set; } = string.Empty;
        public string? TechnicianName { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // Navigation properties for detailed display
        public ApplicationUser? User { get; set; }
        public Vehicle? Vehicle { get; set; }
        public ServiceType? ServiceType { get; set; }
        public Technician? Technician { get; set; }

        // Computed
        public string StatusDisplay => Status.ToString().Replace("_", " ");
        public string StatusColor => Status switch
        {
            AppointmentStatus.Pending => "warning",
            AppointmentStatus.Confirmed => "primary",
            AppointmentStatus.InProgress => "info",
            AppointmentStatus.Completed => "success",
            AppointmentStatus.Cancelled => "danger",
            _ => "secondary"
        };
    }

    /// <summary>
    /// ViewModel for listing appointments
    /// </summary>
    public class AppointmentListViewModel
    {
        public List<AppointmentDetailViewModel> Appointments { get; set; } = new();
        public AppointmentStatus? FilterStatus { get; set; }
        public DateTime? FilterDateFrom { get; set; }
        public DateTime? FilterDateTo { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// ViewModel for admin appointment management
    /// </summary>
    public class AppointmentManageViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Durum")]
        public AppointmentStatus Status { get; set; }

        [Display(Name = "Teknisyen")]
        public int? TechnicianId { get; set; }

        [StringLength(500)]
        [Display(Name = "Teknisyen Notu")]
        public string? TechnicianNotes { get; set; }

        [StringLength(500)]
        [Display(Name = "İptal Sebebi")]
        public string? CancellationReason { get; set; }

        // For display and dropdowns
        public AppointmentDetailViewModel? AppointmentDetails { get; set; }
        public List<Technician> AvailableTechnicians { get; set; } = new();
    }
}
