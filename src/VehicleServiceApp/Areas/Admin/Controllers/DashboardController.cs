using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

            if (filter.StartDate != default && filter.EndDate != default)
            {
                filter.Results = await _dashboardService.GetReportDataAsync(
                    filter.StartDate, 
                    filter.EndDate, 
                    filter.Status, 
                    filter.ServiceTypeId, 
                    filter.TechnicianId);
            }

            ViewData["Title"] = "Raporlar";
            return View(filter);
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
    }
}
