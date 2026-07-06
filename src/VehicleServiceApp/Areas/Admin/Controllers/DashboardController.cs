using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Dashboard Controller for Admin Area
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IServiceTypeService _serviceTypeService;
        private readonly ITechnicianService _technicianService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IDashboardService dashboardService,
            IServiceTypeService serviceTypeService,
            ITechnicianService technicianService,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _serviceTypeService = serviceTypeService;
            _technicianService = technicianService;
            _userManager = userManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync();
            ViewData["Title"] = "Yönetim Paneli";
            return View(dashboard);
        }

        // GET: Admin/Dashboard/Reports
        public async Task<IActionResult> Reports(ReportFilterViewModel? filter)
        {
            filter ??= new ReportFilterViewModel();

            filter.ServiceTypes = (await _serviceTypeService.GetAllServiceTypesAsync()).ToList();
            filter.Technicians = (await _technicianService.GetAllTechniciansAsync()).ToList();

            if (filter.StartDate.Date > filter.EndDate.Date)
            {
                TempData["Error"] = "Başlangıç tarihi bitiş tarihinden sonra olamaz.";
                ViewData["Title"] = "Raporlar";
                return View(filter);
            }

            if (filter.StartDate != default && filter.EndDate != default)
            {
                filter.Results = await _dashboardService.GetReportDataAsync(
                    filter.StartDate, 
                    filter.EndDate, 
                    filter.Status, 
                    filter.ServiceTypeId, 
                    filter.TechnicianId);

                filter.DetailedAppointments = await _dashboardService.GetReportAppointmentsAsync(
                    filter.StartDate,
                    filter.EndDate,
                    filter.Status,
                    filter.ServiceTypeId,
                    filter.TechnicianId);

                filter.TechnicianStatistics = filter.Results.TechnicianStatistics;
            }

            ViewData["Title"] = "Raporlar";
            return View(filter);
        }

        // GET: Admin/Dashboard/ExportReportsCsv
        [HttpGet]
        public async Task<IActionResult> ExportReportsCsv(ReportFilterViewModel filter)
        {
            if (filter.StartDate.Date > filter.EndDate.Date)
            {
                TempData["Error"] = "Başlangıç tarihi bitiş tarihinden sonra olamaz.";
                return RedirectToAction(nameof(Reports), new
                {
                    filter.StartDate,
                    filter.EndDate,
                    filter.Status,
                    filter.ServiceTypeId,
                    filter.TechnicianId
                });
            }

            var appointments = await _dashboardService.GetReportAppointmentsAsync(
                filter.StartDate,
                filter.EndDate,
                filter.Status,
                filter.ServiceTypeId,
                filter.TechnicianId);

            var csv = new StringBuilder();
            csv.AppendLine("Randevu No,Tarih,Saat,Durum,Müşteri,E-posta,Araç,Hizmet,Teknisyen,Tutar");

            foreach (var appointment in appointments)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(appointment.Id.ToString()),
                    EscapeCsv(appointment.AppointmentDate.ToString("dd.MM.yyyy")),
                    EscapeCsv(appointment.AppointmentTime.ToString(@"hh\:mm")),
                    EscapeCsv(appointment.StatusDisplay),
                    EscapeCsv(appointment.CustomerName),
                    EscapeCsv(appointment.CustomerEmail),
                    EscapeCsv(appointment.VehicleInfo),
                    EscapeCsv(appointment.ServiceTypeName),
                    EscapeCsv(appointment.TechnicianName ?? "Atanmadı"),
                    EscapeCsv(appointment.ServicePrice.ToString("F2"))));
            }

            var fileName = $"randevu-raporu-{filter.StartDate:yyyyMMdd}-{filter.EndDate:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // GET: Admin/Dashboard/GetChartData
        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            var last7Days = await _dashboardService.GetLast7DaysDataAsync();
            var serviceStats = await _dashboardService.GetServiceTypeStatsAsync();

            return Json(new
            {
                daily = last7Days,
                services = serviceStats
            });
        }

        private static string EscapeCsv(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
