using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Data;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Services
{
    /// <summary>
    /// Dashboard Service Implementation - Scoped Lifetime
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var dashboard = new DashboardViewModel
            {
                // Total counts
                TotalUsers = await _context.Users.CountAsync(),
                TotalVehicles = await _context.Vehicles.CountAsync(v => v.IsActive),
                TotalAppointments = await _context.Appointments.CountAsync(),
                TotalTechnicians = await _context.Technicians.CountAsync(t => t.IsActive),
                TotalServiceTypes = await _context.ServiceTypes.CountAsync(s => s.IsActive),

                // Status counts
                PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
                ApprovedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed),
                InProgressAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.InProgress),
                CompletedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled),

                // Today's data
                TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today),
                TodayCompletedAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today && a.Status == AppointmentStatus.Completed),
                TodayRevenue = await _context.Appointments
                    .Where(a => a.AppointmentDate == today && a.Status == AppointmentStatus.Completed)
                    .Include(a => a.ServiceType)
                    .SumAsync(a => a.ServiceType != null ? a.ServiceType.Price : 0),

                // Weekly data
                WeeklyAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate >= weekStart && a.AppointmentDate <= today),
                WeeklyCompletedAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate >= weekStart && a.AppointmentDate <= today && a.Status == AppointmentStatus.Completed),
                WeeklyRevenue = await _context.Appointments
                    .Where(a => a.AppointmentDate >= weekStart && a.AppointmentDate <= today && a.Status == AppointmentStatus.Completed)
                    .Include(a => a.ServiceType)
                    .SumAsync(a => a.ServiceType != null ? a.ServiceType.Price : 0),

                // Monthly data
                MonthlyAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate >= monthStart && a.AppointmentDate <= today),
                MonthlyCompletedAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate >= monthStart && a.AppointmentDate <= today && a.Status == AppointmentStatus.Completed),
                MonthlyRevenue = await _context.Appointments
                    .Where(a => a.AppointmentDate >= monthStart && a.AppointmentDate <= today && a.Status == AppointmentStatus.Completed)
                    .Include(a => a.ServiceType)
                    .SumAsync(a => a.ServiceType != null ? a.ServiceType.Price : 0)
            };

            // Recent appointments
            dashboard.RecentAppointments = await GetRecentAppointmentsAsync(10);

            // Upcoming appointments for today
            dashboard.UpcomingTodayAppointments = await GetTodayUpcomingAppointmentsAsync();

            // Chart data
            dashboard.Last7DaysAppointments = await GetLast7DaysDataAsync();
            dashboard.ServiceTypeStatistics = await GetServiceTypeStatsAsync();

            return dashboard;
        }

        public async Task<DashboardViewModel> GetReportDataAsync(DateTime startDate, DateTime endDate, AppointmentStatus? status = null, int? serviceTypeId = null, int? technicianId = null)
        {
            var query = _context.Appointments
                .Include(a => a.ServiceType)
                .Where(a => a.AppointmentDate >= startDate.Date && a.AppointmentDate <= endDate.Date);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (serviceTypeId.HasValue)
                query = query.Where(a => a.ServiceTypeId == serviceTypeId.Value);

            if (technicianId.HasValue)
                query = query.Where(a => a.TechnicianId == technicianId.Value);

            var appointments = await query.ToListAsync();

            return new DashboardViewModel
            {
                TotalAppointments = appointments.Count,
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                MonthlyRevenue = appointments.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.ServiceType?.Price ?? 0)
            };
        }

        public async Task<List<DailyAppointmentData>> GetLast7DaysDataAsync()
        {
            var result = new List<DailyAppointmentData>();
            var today = DateTime.Today;

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayAppointments = await _context.Appointments
                    .Include(a => a.ServiceType)
                    .Where(a => a.AppointmentDate == date)
                    .ToListAsync();

                result.Add(new DailyAppointmentData
                {
                    Date = date,
                    DayName = date.ToString("ddd", new System.Globalization.CultureInfo("tr-TR")),
                    Count = dayAppointments.Count,
                    CompletedCount = dayAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                    Revenue = dayAppointments.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.ServiceType?.Price ?? 0)
                });
            }

            return result;
        }

        public async Task<List<ServiceTypeStats>> GetServiceTypeStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Appointments
                .Include(a => a.ServiceType)
                .Where(a => a.ServiceType != null);

            if (startDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= endDate.Value.Date);

            var grouped = await query
                .GroupBy(a => new { a.ServiceTypeId, a.ServiceType!.Name, a.ServiceType.ColorCode })
                .Select(g => new ServiceTypeStats
                {
                    ServiceName = g.Key.Name,
                    AppointmentCount = g.Count(),
                    TotalRevenue = g.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.ServiceType != null ? a.ServiceType.Price : 0),
                    ColorCode = g.Key.ColorCode
                })
                .OrderByDescending(s => s.AppointmentCount)
                .ToListAsync();

            return grouped;
        }

        private async Task<List<AppointmentDetailViewModel>> GetRecentAppointmentsAsync(int count)
        {
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();

            return appointments.Select(a => new AppointmentDetailViewModel
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                Status = a.Status,
                CustomerNotes = a.CustomerNotes,
                TechnicianNotes = a.TechnicianNotes,
                CreatedAt = a.CreatedAt,
                VehicleInfo = a.Vehicle?.VehicleInfo ?? "N/A",
                ServiceTypeName = a.ServiceType?.Name ?? "N/A",
                ServicePrice = a.ServiceType?.Price ?? 0,
                ServiceDuration = a.ServiceType?.FormattedDuration ?? "N/A",
                TechnicianName = a.Technician?.FullName,
                CustomerName = a.User?.FullName ?? "N/A",
                CustomerEmail = a.User?.Email ?? "N/A"
            }).ToList();
        }

        private async Task<List<AppointmentDetailViewModel>> GetTodayUpcomingAppointmentsAsync()
        {
            var today = DateTime.Today;
            var currentTime = DateTime.UtcNow.TimeOfDay;

            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceType)
                .Include(a => a.Technician)
                .Where(a => a.AppointmentDate == today)
                .Where(a => a.AppointmentTime >= currentTime)
                .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Completed)
                .OrderBy(a => a.AppointmentTime)
                .Take(5)
                .ToListAsync();

            return appointments.Select(a => new AppointmentDetailViewModel
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                Status = a.Status,
                VehicleInfo = a.Vehicle?.VehicleInfo ?? "N/A",
                ServiceTypeName = a.ServiceType?.Name ?? "N/A",
                TechnicianName = a.Technician?.FullName,
                CustomerName = a.User?.FullName ?? "N/A"
            }).ToList();
        }
    }
}

