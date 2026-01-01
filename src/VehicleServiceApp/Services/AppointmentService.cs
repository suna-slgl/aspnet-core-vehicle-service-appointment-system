using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Data;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Services
{
    /// <summary>
    /// Appointment Service Implementation - Scoped Lifetime
    /// </summary>
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(string userId)
        {
            return await _context.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(AppointmentStatus status)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .Where(a => a.AppointmentDate >= startDate.Date && a.AppointmentDate <= endDate.Date)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync()
        {
            var today = DateTime.Today;
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .Where(a => a.AppointmentDate == today)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int days = 7)
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(days);
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate <= endDate)
                .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Completed)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Appointment?> GetAppointmentByIdAndUserAsync(int id, string userId)
        {
            return await _context.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            appointment.CreatedAt = DateTime.Now;
            appointment.Status = AppointmentStatus.Pending;
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            appointment.UpdatedAt = DateTime.Now;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, AppointmentStatus status, string? notes = null, string? cancellationReason = null)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(notes))
                appointment.TechnicianNotes = notes;

            if (!string.IsNullOrEmpty(cancellationReason))
                appointment.CancellationReason = cancellationReason;

            switch (status)
            {
                case AppointmentStatus.Confirmed:
                    appointment.ApprovedAt = DateTime.Now;
                    break;
                case AppointmentStatus.Completed:
                    appointment.CompletedAt = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTechnicianAsync(int appointmentId, int technicianId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return false;

            appointment.TechnicianId = technicianId;
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsTimeSlotAvailableAsync(DateTime date, TimeSpan time, int? technicianId = null, int? excludeAppointmentId = null)
        {
            var query = _context.Appointments
                .Where(a => a.AppointmentDate == date.Date)
                .Where(a => a.AppointmentTime == time)
                .Where(a => a.Status != AppointmentStatus.Cancelled);

            if (technicianId.HasValue)
                query = query.Where(a => a.TechnicianId == technicianId.Value);

            if (excludeAppointmentId.HasValue)
                query = query.Where(a => a.Id != excludeAppointmentId.Value);

            var existingCount = await query.CountAsync();
            
            // If no technician specified, allow max 5 appointments per time slot
            if (!technicianId.HasValue)
                return existingCount < 5;

            // If technician specified, only one appointment per time slot
            return existingCount == 0;
        }

        public async Task<IEnumerable<string>> GetAvailableTimeSlotsAsync(DateTime date, int? technicianId = null)
        {
            var allTimeSlots = new List<string>();
            for (int hour = 8; hour < 18; hour++)
            {
                allTimeSlots.Add($"{hour:00}:00");
                allTimeSlots.Add($"{hour:00}:30");
            }

            var availableSlots = new List<string>();
            foreach (var slot in allTimeSlots)
            {
                var time = TimeSpan.Parse(slot);
                if (await IsTimeSlotAvailableAsync(date, time, technicianId))
                {
                    availableSlots.Add(slot);
                }
            }

            return availableSlots;
        }

        public async Task<int> GetAppointmentCountByUserAsync(string userId)
        {
            return await _context.Appointments.CountAsync(a => a.UserId == userId);
        }

        public async Task<int> GetCompletedAppointmentCountByUserAsync(string userId)
        {
            return await _context.Appointments
                .CountAsync(a => a.UserId == userId && a.Status == AppointmentStatus.Completed);
        }
    }
}
