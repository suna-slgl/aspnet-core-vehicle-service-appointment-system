using System.ComponentModel.DataAnnotations;
using VehicleServiceApp.Models;

namespace VehicleServiceApp.ViewModels
{
    /// <summary>
    /// Dashboard summary ViewModel for admin area
    /// </summary>
    public class DashboardViewModel
    {
        // Summary counts
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalTechnicians { get; set; }
        public int TotalServiceTypes { get; set; }

        // Appointment status counts
        public int PendingAppointments { get; set; }
        public int ApprovedAppointments { get; set; }
        public int InProgressAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }

        // Today's data
        public int TodayAppointments { get; set; }
        public int TodayCompletedAppointments { get; set; }
        public decimal TodayRevenue { get; set; }

        // Weekly data
        public int WeeklyAppointments { get; set; }
        public int WeeklyCompletedAppointments { get; set; }
        public decimal WeeklyRevenue { get; set; }

        // Monthly data
        public int MonthlyAppointments { get; set; }
        public int MonthlyCompletedAppointments { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Recent appointments
        public List<AppointmentDetailViewModel> RecentAppointments { get; set; } = new();

        // Upcoming appointments for today
        public List<AppointmentDetailViewModel> UpcomingTodayAppointments { get; set; } = new();

        // Chart data
        public List<DailyAppointmentData> Last7DaysAppointments { get; set; } = new();
        public List<ServiceTypeStats> ServiceTypeStatistics { get; set; } = new();
        public List<PopularServiceViewModel> PopularServices { get; set; } = new();
    }

    /// <summary>
    /// Daily appointment data for charts
    /// </summary>
    public class DailyAppointmentData
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int Count { get; set; }
        public int CompletedCount { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Service type statistics
    /// </summary>
    public class ServiceTypeStats
    {
        public string ServiceName { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? ColorCode { get; set; }
    }

    /// <summary>
    /// Popular service view model for dashboard
    /// </summary>
    public class PopularServiceViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string? ColorCode { get; set; }
        public int AppointmentCount { get; set; }
    }

    /// <summary>
    /// Report filter ViewModel
    /// </summary>
    public class ReportFilterViewModel
    {
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Display(Name = "Durum")]
        public AppointmentStatus? Status { get; set; }

        [Display(Name = "Servis Türü")]
        public int? ServiceTypeId { get; set; }

        [Display(Name = "Teknisyen")]
        public int? TechnicianId { get; set; }

        // Report results
        public DashboardViewModel? Results { get; set; }
        public List<AppointmentDetailViewModel> DetailedAppointments { get; set; } = new();

        // Filter options
        public List<ServiceType> ServiceTypes { get; set; } = new();
        public List<Technician> Technicians { get; set; } = new();
    }

    /// <summary>
    /// User dashboard summary ViewModel (non-admin)
    /// </summary>
    public class UserDashboardViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public int TotalVehicles { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public List<AppointmentDetailViewModel> UpcomingAppointments { get; set; } = new();
    }
}
