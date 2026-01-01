using VehicleServiceApp.Models;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Services.Interfaces
{
    /// <summary>
    /// Interface for Vehicle Service
    /// </summary>
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<IEnumerable<Vehicle>> GetVehiclesByUserIdAsync(string userId);
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task<Vehicle?> GetVehicleByIdAndUserAsync(int id, string userId);
        Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
        Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle);
        Task<bool> DeleteVehicleAsync(int id);
        Task<bool> IsLicensePlateExistsAsync(string licensePlate, int? excludeId = null);
        Task<int> GetVehicleCountByUserAsync(string userId);
    }

    /// <summary>
    /// Interface for Appointment Service
    /// </summary>
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(string userId);
        Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(AppointmentStatus status);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync();
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int days = 7);
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<Appointment?> GetAppointmentByIdAndUserAsync(int id, string userId);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<bool> UpdateStatusAsync(int id, AppointmentStatus status, string? notes = null, string? cancellationReason = null);
        Task<bool> AssignTechnicianAsync(int appointmentId, int technicianId);
        Task<bool> IsTimeSlotAvailableAsync(DateTime date, TimeSpan time, int? technicianId = null, int? excludeAppointmentId = null);
        Task<IEnumerable<string>> GetAvailableTimeSlotsAsync(DateTime date, int? technicianId = null);
        Task<int> GetAppointmentCountByUserAsync(string userId);
        Task<int> GetCompletedAppointmentCountByUserAsync(string userId);
    }

    /// <summary>
    /// Interface for ServiceType Service
    /// </summary>
    public interface IServiceTypeService
    {
        Task<IEnumerable<ServiceType>> GetAllServiceTypesAsync();
        Task<IEnumerable<ServiceType>> GetActiveServiceTypesAsync();
        Task<ServiceType?> GetServiceTypeByIdAsync(int id);
        Task<ServiceType> CreateServiceTypeAsync(ServiceType serviceType);
        Task<ServiceType> UpdateServiceTypeAsync(ServiceType serviceType);
        Task<bool> DeleteServiceTypeAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
    }

    /// <summary>
    /// Interface for Technician Service
    /// </summary>
    public interface ITechnicianService
    {
        Task<IEnumerable<Technician>> GetAllTechniciansAsync();
        Task<IEnumerable<Technician>> GetActiveTechniciansAsync();
        Task<Technician?> GetTechnicianByIdAsync(int id);
        Task<Technician> CreateTechnicianAsync(Technician technician);
        Task<Technician> UpdateTechnicianAsync(Technician technician);
        Task<bool> DeleteTechnicianAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<IEnumerable<Technician>> GetAvailableTechniciansAsync(DateTime date, TimeSpan time);
    }

    /// <summary>
    /// Interface for Dashboard/Report Service
    /// </summary>
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<DashboardViewModel> GetReportDataAsync(DateTime startDate, DateTime endDate, AppointmentStatus? status = null, int? serviceTypeId = null, int? technicianId = null);
        Task<List<DailyAppointmentData>> GetLast7DaysDataAsync();
        Task<List<ServiceTypeStats>> GetServiceTypeStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// Interface for File Upload Service
    /// </summary>
    public interface IFileService
    {
        Task<string> UploadFileAsync(Microsoft.AspNetCore.Http.IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string filePath);
        Task<string?> GetFilePathAsync(string folder, string fileName);
    }
}
