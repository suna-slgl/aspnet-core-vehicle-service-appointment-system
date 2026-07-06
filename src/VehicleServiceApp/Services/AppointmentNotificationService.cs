using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Data;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Services
{
    /// <summary>
    /// Sends customer notifications for appointment lifecycle events.
    /// </summary>
    public class AppointmentNotificationService : IAppointmentNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AppointmentNotificationService> _logger;

        public AppointmentNotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<AppointmentNotificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendAppointmentCreatedAsync(int appointmentId)
        {
            var appointment = await GetAppointmentAsync(appointmentId);
            if (appointment == null)
                return;

            var subject = "Randevu talebiniz alındı";
            var body = BuildAppointmentBody(
                appointment,
                "Randevu talebiniz başarıyla alındı. Randevunuz onaylandığında tekrar bilgilendirileceksiniz.");

            await SendAsync(appointment, subject, body);
        }

        public async Task SendAppointmentStatusChangedAsync(int appointmentId, AppointmentStatus status)
        {
            var appointment = await GetAppointmentAsync(appointmentId);
            if (appointment == null)
                return;

            var subject = $"Randevu durumunuz güncellendi: {GetStatusText(status)}";
            var body = BuildAppointmentBody(
                appointment,
                $"Randevu durumunuz {GetStatusText(status)} olarak güncellendi.");

            await SendAsync(appointment, subject, body);
        }

        private async Task<Appointment?> GetAppointmentAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        private async Task SendAsync(Appointment appointment, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(appointment.User?.Email))
            {
                _logger.LogWarning("Appointment notification skipped because appointment {AppointmentId} has no customer email.", appointment.Id);
                return;
            }

            await _emailService.SendAsync(appointment.User.Email, subject, body);
        }

        private static string BuildAppointmentBody(Appointment appointment, string message)
        {
            var vehicle = appointment.Vehicle?.VehicleInfo ?? "Araç bilgisi yok";
            var service = appointment.ServiceType?.Name ?? "Hizmet bilgisi yok";
            var technician = appointment.Technician?.FullName ?? "Henüz atanmadı";

            return $@"
<p>Merhaba {appointment.User?.FullName},</p>
<p>{message}</p>
<p>
    <strong>Tarih:</strong> {appointment.AppointmentDate:dd.MM.yyyy}<br />
    <strong>Saat:</strong> {appointment.AppointmentTime:hh\:mm}<br />
    <strong>Araç:</strong> {vehicle}<br />
    <strong>Hizmet:</strong> {service}<br />
    <strong>Teknisyen:</strong> {technician}
</p>";
        }

        private static string GetStatusText(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Pending => "Beklemede",
                AppointmentStatus.Confirmed => "Onaylandı",
                AppointmentStatus.InProgress => "Devam Ediyor",
                AppointmentStatus.Completed => "Tamamlandı",
                AppointmentStatus.Cancelled => "İptal Edildi",
                _ => status.ToString()
            };
        }
    }
}
