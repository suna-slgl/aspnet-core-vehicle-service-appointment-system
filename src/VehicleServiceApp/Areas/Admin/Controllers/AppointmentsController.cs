using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Filters;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Appointments Management Controller for Admin Area
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(AdminActionFilter))]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ITechnicianService _technicianService;

        public AppointmentsController(
            IAppointmentService appointmentService,
            ITechnicianService technicianService)
        {
            _appointmentService = appointmentService;
            _technicianService = technicianService;
        }

        // GET: Admin/Appointments
        public async Task<IActionResult> Index(AppointmentStatus? status, DateTime? dateFrom, DateTime? dateTo, int page = 1)
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            // Apply filters
            if (status.HasValue)
                appointments = appointments.Where(a => a.Status == status.Value);

            if (dateFrom.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate >= dateFrom.Value.Date);

            if (dateTo.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate <= dateTo.Value.Date);

            var totalCount = appointments.Count();
            var pageSize = 15;
            var pagedAppointments = appointments
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var viewModel = new AppointmentListViewModel
            {
                Appointments = pagedAppointments.Select(a => new AppointmentDetailViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status,
                    CustomerNotes = a.CustomerNotes,
                    TechnicianNotes = a.TechnicianNotes,
                    CancellationReason = a.CancellationReason,
                    CreatedAt = a.CreatedAt,
                    ApprovedAt = a.ApprovedAt,
                    CompletedAt = a.CompletedAt,
                    VehicleInfo = a.Vehicle?.VehicleInfo ?? "N/A",
                    ServiceTypeName = a.ServiceType?.Name ?? "N/A",
                    ServicePrice = a.ServiceType?.Price ?? 0,
                    ServiceDuration = a.ServiceType?.FormattedDuration ?? "N/A",
                    TechnicianName = a.Technician?.FullName,
                    CustomerName = a.User?.FullName ?? "N/A",
                    CustomerEmail = a.User?.Email ?? "N/A"
                }).ToList(),
                FilterStatus = status,
                FilterDateFrom = dateFrom,
                FilterDateTo = dateTo,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            ViewData["Title"] = "Randevu Yönetimi";
            return View(viewModel);
        }

        // GET: Admin/Appointments/Pending
        public async Task<IActionResult> Pending()
        {
            var appointments = await _appointmentService.GetAppointmentsByStatusAsync(AppointmentStatus.Pending);

            var viewModel = appointments.Select(a => new AppointmentDetailViewModel
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                Status = a.Status,
                CustomerNotes = a.CustomerNotes,
                VehicleInfo = a.Vehicle?.VehicleInfo ?? "N/A",
                ServiceTypeName = a.ServiceType?.Name ?? "N/A",
                ServicePrice = a.ServiceType?.Price ?? 0,
                CustomerName = a.User?.FullName ?? "N/A",
                CustomerEmail = a.User?.Email ?? "N/A"
            }).ToList();

            ViewData["Title"] = "Bekleyen Randevular";
            return View(viewModel);
        }

        // GET: Admin/Appointments/Today
        public async Task<IActionResult> Today()
        {
            var appointments = await _appointmentService.GetTodayAppointmentsAsync();

            var viewModel = appointments.Select(a => new AppointmentDetailViewModel
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                Status = a.Status,
                CustomerNotes = a.CustomerNotes,
                TechnicianNotes = a.TechnicianNotes,
                VehicleInfo = a.Vehicle?.VehicleInfo ?? "N/A",
                ServiceTypeName = a.ServiceType?.Name ?? "N/A",
                TechnicianName = a.Technician?.FullName,
                CustomerName = a.User?.FullName ?? "N/A"
            }).ToList();

            ViewData["Title"] = "Bugünkü Randevular";
            return View(viewModel);
        }

        // GET: Admin/Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var technicians = await _technicianService.GetActiveTechniciansAsync();

            var viewModel = new AppointmentManageViewModel
            {
                Id = appointment.Id,
                Status = appointment.Status,
                TechnicianId = appointment.TechnicianId,
                TechnicianNotes = appointment.TechnicianNotes,
                AvailableTechnicians = technicians.ToList(),
                AppointmentDetails = new AppointmentDetailViewModel
                {
                    Id = appointment.Id,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status,
                    CustomerNotes = appointment.CustomerNotes,
                    TechnicianNotes = appointment.TechnicianNotes,
                    CancellationReason = appointment.CancellationReason,
                    CreatedAt = appointment.CreatedAt,
                    ApprovedAt = appointment.ApprovedAt,
                    CompletedAt = appointment.CompletedAt,
                    VehicleInfo = appointment.Vehicle?.VehicleInfo ?? "N/A",
                    ServiceTypeName = appointment.ServiceType?.Name ?? "N/A",
                    ServicePrice = appointment.ServiceType?.Price ?? 0,
                    ServiceDuration = appointment.ServiceType?.FormattedDuration ?? "N/A",
                    TechnicianName = appointment.Technician?.FullName,
                    CustomerName = appointment.User?.FullName ?? "N/A",
                    CustomerEmail = appointment.User?.Email ?? "N/A"
                }
            };

            ViewData["Title"] = "Randevu Detayı";
            return View(viewModel);
        }

        // POST: Admin/Appointments/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int? technicianId, string? notes)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status != AppointmentStatus.Pending)
            {
                TempData["Error"] = "Sadece beklemede olan randevular onaylanabilir.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Assign technician if provided
            if (technicianId.HasValue)
            {
                await _appointmentService.AssignTechnicianAsync(id, technicianId.Value);
            }

            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.Confirmed, notes);
            TempData["Success"] = "Randevu başarıyla onaylandı.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Appointments/Start/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                TempData["Error"] = "Sadece onaylanmış randevular başlatılabilir.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.InProgress);
            TempData["Success"] = "Servis işlemi başlatıldı.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Appointments/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, string? notes)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status != AppointmentStatus.InProgress)
            {
                TempData["Error"] = "Sadece devam eden randevular tamamlanabilir.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.Completed, notes);
            TempData["Success"] = "Servis işlemi tamamlandı.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Appointments/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            {
                TempData["Error"] = "Bu randevu iptal edilemez.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.Cancelled, null, reason ?? "Yönetici tarafından iptal edildi");
            TempData["Success"] = "Randevu iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Appointments/AssignTechnician/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTechnician(int id, int technicianId)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            await _appointmentService.AssignTechnicianAsync(id, technicianId);
            TempData["Success"] = "Teknisyen atandı.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
