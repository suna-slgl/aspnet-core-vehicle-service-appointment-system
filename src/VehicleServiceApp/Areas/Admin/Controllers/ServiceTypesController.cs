using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Service Types Management Controller for Admin Area
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceTypesController : Controller
    {
        private readonly IServiceTypeService _serviceTypeService;

        public ServiceTypesController(IServiceTypeService serviceTypeService)
        {
            _serviceTypeService = serviceTypeService;
        }

        // GET: Admin/ServiceTypes
        public async Task<IActionResult> Index()
        {
            var serviceTypes = await _serviceTypeService.GetAllServiceTypesAsync();
            ViewData["Title"] = "Servis Türleri";
            return View(serviceTypes);
        }

        // GET: Admin/ServiceTypes/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Yeni Servis Türü";
            return View(new ServiceType
            {
                EstimatedDurationMinutes = 60,
                SortOrder = 1
            });
        }

        // POST: Admin/ServiceTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceType model)
        {
            if (ModelState.IsValid)
            {
                await _serviceTypeService.CreateServiceTypeAsync(model);
                TempData["Success"] = "Servis türü başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Yeni Servis Türü";
            return View(model);
        }

        // GET: Admin/ServiceTypes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var serviceType = await _serviceTypeService.GetServiceTypeByIdAsync(id);
            if (serviceType == null)
            {
                TempData["Error"] = "Servis türü bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = $"Servis Türü Düzenle - {serviceType.Name}";
            return View(serviceType);
        }

        // POST: Admin/ServiceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceType model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var serviceType = await _serviceTypeService.GetServiceTypeByIdAsync(id);
                if (serviceType == null)
                {
                    TempData["Error"] = "Servis türü bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                serviceType.Name = model.Name;
                serviceType.Description = model.Description;
                serviceType.EstimatedDurationMinutes = model.EstimatedDurationMinutes;
                serviceType.Price = model.Price;
                serviceType.IconClass = model.IconClass;
                serviceType.ColorCode = model.ColorCode;
                serviceType.SortOrder = model.SortOrder;
                serviceType.IsActive = model.IsActive;

                await _serviceTypeService.UpdateServiceTypeAsync(serviceType);
                TempData["Success"] = "Servis türü başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Servis Türü Düzenle";
            return View(model);
        }

        // POST: Admin/ServiceTypes/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _serviceTypeService.ToggleActiveAsync(id);
            if (result)
                TempData["Success"] = "Durum güncellendi.";
            else
                TempData["Error"] = "Servis türü bulunamadı.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ServiceTypes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceTypeService.DeleteServiceTypeAsync(id);
            if (result)
                TempData["Success"] = "Servis türü silindi.";
            else
                TempData["Error"] = "Servis türü silinemedi.";

            return RedirectToAction(nameof(Index));
        }
    }
}
