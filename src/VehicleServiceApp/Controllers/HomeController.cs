using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Controllers
{
    /// <summary>
    /// Home Controller - Main entry point for the application
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServiceTypeService _serviceTypeService;

        public HomeController(ILogger<HomeController> logger, IServiceTypeService serviceTypeService)
        {
            _logger = logger;
            _serviceTypeService = serviceTypeService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Ana Sayfa";
            var serviceTypes = await _serviceTypeService.GetActiveServiceTypesAsync();
            return View(serviceTypes);
        }

        public IActionResult About()
        {
            ViewData["Title"] = "Hakkımızda";
            ViewBag.CompanyName = "Araç Servis Randevu Sistemi";
            ViewBag.Slogan = "Güvenilir, Hızlı ve Profesyonel Servis";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "İletişim";
            ViewBag.Address = "İstanbul, Türkiye";
            ViewBag.PhoneNumber = "+90 212 XXX XX XX";
            ViewBag.Email = "info@aracservis.com";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Title"] = "Gizlilik Politikası";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
