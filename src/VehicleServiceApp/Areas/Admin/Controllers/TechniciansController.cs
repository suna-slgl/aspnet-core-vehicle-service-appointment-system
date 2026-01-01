using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Technicians Management Controller for Admin Area
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TechniciansController : Controller
    {
        private readonly ITechnicianService _technicianService;
        private readonly IFileService _fileService;

        public TechniciansController(ITechnicianService technicianService, IFileService fileService)
        {
            _technicianService = technicianService;
            _fileService = fileService;
        }

        // GET: Admin/Technicians
        public async Task<IActionResult> Index()
        {
            var technicians = await _technicianService.GetAllTechniciansAsync();
            ViewData["Title"] = "Teknisyen Yönetimi";
            return View(technicians);
        }

        // GET: Admin/Technicians/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var technician = await _technicianService.GetTechnicianByIdAsync(id);
            if (technician == null)
            {
                TempData["Error"] = "Teknisyen bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = $"Teknisyen Detayı - {technician.FullName}";
            return View(technician);
        }

        // GET: Admin/Technicians/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Yeni Teknisyen";
            return View(new Technician());
        }

        // POST: Admin/Technicians/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Technician model, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        model.ImagePath = await _fileService.UploadFileAsync(imageFile, "technicians");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        ViewData["Title"] = "Yeni Teknisyen";
                        return View(model);
                    }
                }

                await _technicianService.CreateTechnicianAsync(model);
                TempData["Success"] = "Teknisyen başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Yeni Teknisyen";
            return View(model);
        }

        // GET: Admin/Technicians/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var technician = await _technicianService.GetTechnicianByIdAsync(id);
            if (technician == null)
            {
                TempData["Error"] = "Teknisyen bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = $"Teknisyen Düzenle - {technician.FullName}";
            return View(technician);
        }

        // POST: Admin/Technicians/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Technician model, IFormFile? imageFile)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var technician = await _technicianService.GetTechnicianByIdAsync(id);
                if (technician == null)
                {
                    TempData["Error"] = "Teknisyen bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                technician.FirstName = model.FirstName;
                technician.LastName = model.LastName;
                technician.PhoneNumber = model.PhoneNumber;
                technician.Email = model.Email;
                technician.Specialization = model.Specialization;
                technician.ExperienceYears = model.ExperienceYears;
                technician.WorkStartTime = model.WorkStartTime;
                technician.WorkEndTime = model.WorkEndTime;
                technician.IsActive = model.IsActive;

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(technician.ImagePath))
                        {
                            await _fileService.DeleteFileAsync(technician.ImagePath);
                        }

                        technician.ImagePath = await _fileService.UploadFileAsync(imageFile, "technicians");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        ViewData["Title"] = "Teknisyen Düzenle";
                        return View(model);
                    }
                }

                await _technicianService.UpdateTechnicianAsync(technician);
                TempData["Success"] = "Teknisyen bilgileri güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Teknisyen Düzenle";
            return View(model);
        }

        // POST: Admin/Technicians/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _technicianService.ToggleActiveAsync(id);
            if (result)
                TempData["Success"] = "Durum güncellendi.";
            else
                TempData["Error"] = "Teknisyen bulunamadı.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Technicians/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _technicianService.DeleteTechnicianAsync(id);
            if (result)
                TempData["Success"] = "Teknisyen silindi.";
            else
                TempData["Error"] = "Teknisyen silinemedi.";

            return RedirectToAction(nameof(Index));
        }
    }
}
