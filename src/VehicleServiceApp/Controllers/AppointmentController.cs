using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Controllers
{
    /// <summary>
    /// Appointment Controller - Handles appointment management for authenticated users
    /// </summary>
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IVehicleService _vehicleService;
        private readonly IServiceTypeService _serviceTypeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(
            IAppointmentService appointmentService,
            IVehicleService vehicleService,
            IServiceTypeService serviceTypeService,
            UserManager<ApplicationUser> userManager)
        {
            _appointmentService = appointmentService;
            _vehicleService = vehicleService;
            _serviceTypeService = serviceTypeService;
            _userManager = userManager;
        }

        // GET: Appointment
        public async Task<IActionResult> Index(AppointmentStatus? status, DateTime? dateFrom, DateTime? dateTo, int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var appointments = await _appointmentService.GetAppointmentsByUserIdAsync(userId!);

            // Apply filters
            if (status.HasValue)
                appointments = appointments.Where(a => a.Status == status.Value);

            if (dateFrom.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate >= dateFrom.Value.Date);

            if (dateTo.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate <= dateTo.Value.Date);

            var totalCount = appointments.Count();
            var pageSize = 10;
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
                    TechnicianName = a.Technician?.FullName
                }).ToList(),
                FilterStatus = status,
                FilterDateFrom = dateFrom,
                FilterDateTo = dateTo,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            ViewData["Title"] = "Randevularım";
            return View(viewModel);
        }

        // GET: Appointment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _appointmentService.GetAppointmentByIdAndUserAsync(id, userId!);

            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new AppointmentDetailViewModel
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
                TechnicianName = appointment.Technician?.FullName
            };

            ViewData["Title"] = "Randevu Detayı";
            return View(viewModel);
        }

        // GET: Appointment/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var vehicles = await _vehicleService.GetVehiclesByUserIdAsync(userId!);

            if (!vehicles.Any())
            {
                TempData["Warning"] = "Randevu oluşturmak için önce bir araç eklemeniz gerekiyor.";
                return RedirectToAction("Create", "Vehicle");
            }

            var serviceTypes = await _serviceTypeService.GetActiveServiceTypesAsync();
            var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(DateTime.Today.AddDays(1));

            var viewModel = new AppointmentCreateViewModel
            {
                AvailableVehicles = vehicles.ToList(),
                AvailableServiceTypes = serviceTypes.ToList(),
                AvailableTimeSlots = availableSlots.ToList()
            };

            ViewData["Title"] = "Yeni Randevu";
            return View(viewModel);
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                // Validate vehicle belongs to user
                var vehicle = await _vehicleService.GetVehicleByIdAndUserAsync(model.VehicleId, userId!);
                if (vehicle == null)
                {
                    ModelState.AddModelError("VehicleId", "Geçersiz araç seçimi.");
                }

                // Parse and validate time
                if (!TimeSpan.TryParse(model.AppointmentTime, out var time))
                {
                    ModelState.AddModelError("AppointmentTime", "Geçersiz saat formatı.");
                }
                else
                {
                    // Check if time slot is available
                    if (!await _appointmentService.IsTimeSlotAvailableAsync(model.AppointmentDate, time))
                    {
                        ModelState.AddModelError("AppointmentTime", "Bu tarih ve saat için uygun randevu yeri bulunmamaktadır.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var appointment = new Appointment
                    {
                        UserId = userId!,
                        VehicleId = model.VehicleId,
                        ServiceTypeId = model.ServiceTypeId,
                        AppointmentDate = model.AppointmentDate.Date,
                        AppointmentTime = time,
                        CustomerNotes = model.CustomerNotes,
                        Status = AppointmentStatus.Pending
                    };

                    await _appointmentService.CreateAppointmentAsync(appointment);
                    TempData["Success"] = "Randevunuz başarıyla oluşturuldu. Onay bekleniyor.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Reload dropdown data
            model.AvailableVehicles = (await _vehicleService.GetVehiclesByUserIdAsync(userId!)).ToList();
            model.AvailableServiceTypes = (await _serviceTypeService.GetActiveServiceTypesAsync()).ToList();
            model.AvailableTimeSlots = (await _appointmentService.GetAvailableTimeSlotsAsync(model.AppointmentDate)).ToList();

            ViewData["Title"] = "Yeni Randevu";
            return View(model);
        }

        // GET: Appointment/GetAvailableSlots
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(DateTime date)
        {
            var slots = await _appointmentService.GetAvailableTimeSlotsAsync(date);
            return Json(slots);
        }

        // POST: Appointment/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _appointmentService.GetAppointmentByIdAndUserAsync(id, userId!);

            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Check if appointment can be cancelled
            if (appointment.Status == AppointmentStatus.Completed || 
                appointment.Status == AppointmentStatus.Cancelled ||
                appointment.Status == AppointmentStatus.InProgress)
            {
                TempData["Error"] = "Bu randevu iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }

            // Check if appointment is within 24 hours
            var appointmentDateTime = appointment.AppointmentDate.Date.Add(appointment.AppointmentTime);
            if (appointmentDateTime <= DateTime.Now.AddHours(24))
            {
                TempData["Error"] = "Randevu saatine 24 saatten az kaldığı için iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }

            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.Cancelled, null, reason ?? "Kullanıcı tarafından iptal edildi");
            TempData["Success"] = "Randevu başarıyla iptal edildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
