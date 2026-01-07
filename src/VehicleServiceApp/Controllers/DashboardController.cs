using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Controllers
{
    /// <summary>
    /// User Dashboard (public site - authenticated users)
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVehicleService _vehicleService;
        private readonly IAppointmentService _appointmentService;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IVehicleService vehicleService,
            IAppointmentService appointmentService)
        {
            _userManager = userManager;
            _vehicleService = vehicleService;
            _appointmentService = appointmentService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var totalVehicles = await _vehicleService.GetVehicleCountByUserAsync(user.Id);
            var totalAppointments = await _appointmentService.GetAppointmentCountByUserAsync(user.Id);
            var completedAppointments = await _appointmentService.GetCompletedAppointmentCountByUserAsync(user.Id);
            var userAppointments = await _appointmentService.GetAppointmentsByUserIdAsync(user.Id);

            var upcoming = userAppointments
                .Where(a => a.AppointmentDate.Date >= DateTime.Today && a.Status != AppointmentStatus.Completed && a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .Take(5)
                .Select(a => new AppointmentDetailViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status,
                    ServiceTypeName = a.ServiceType?.Name ?? "N/A",
                    ServicePrice = a.ServiceType?.Price ?? 0,
                    VehicleInfo = a.Vehicle != null ? $"{a.Vehicle.Brand} {a.Vehicle.Model} - {a.Vehicle.LicensePlate}" : "Araç"
                }).ToList();

            var vm = new UserDashboardViewModel
            {
                FirstName = user.FirstName,
                TotalVehicles = totalVehicles,
                TotalAppointments = totalAppointments,
                CompletedAppointments = completedAppointments,
                UpcomingAppointments = upcoming
            };

            ViewData["Title"] = "Panelim";
            return View(vm);
        }
    }
}