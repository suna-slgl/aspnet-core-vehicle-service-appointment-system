using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Controllers
{
    /// <summary>
    /// Vehicle Controller - Handles vehicle management for authenticated users
    /// </summary>
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly IAppointmentService _appointmentService;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehicleController(
            IVehicleService vehicleService,
            IAppointmentService appointmentService,
            IFileService fileService,
            UserManager<ApplicationUser> userManager)
        {
            _vehicleService = vehicleService;
            _appointmentService = appointmentService;
            _fileService = fileService;
            _userManager = userManager;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var userId = _userManager.GetUserId(User);
            var vehicles = await _vehicleService.GetVehiclesByUserIdAsync(userId!);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                vehicles = vehicles.Where(v => 
                    v.LicensePlate.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    v.Brand.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    v.Model.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            var viewModel = new VehicleListViewModel
            {
                Vehicles = vehicles.Select(v => new VehicleDetailViewModel
                {
                    Id = v.Id,
                    LicensePlate = v.LicensePlate,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    Mileage = v.Mileage,
                    FuelType = v.FuelType,
                    ImagePath = v.ImagePath,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    IsActive = v.IsActive,
                    TotalAppointments = v.Appointments.Count,
                    CompletedAppointments = v.Appointments.Count(a => a.Status == AppointmentStatus.Completed),
                    PendingAppointments = v.Appointments.Count(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed)
                }).ToList(),
                TotalCount = vehicles.Count(),
                SearchTerm = searchTerm
            };

            ViewData["Title"] = "Araçlarım";
            return View(viewModel);
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var vehicle = await _vehicleService.GetVehicleByIdAndUserAsync(id, userId!);

            if (vehicle == null)
            {
                TempData["Error"] = "Araç bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new VehicleDetailViewModel
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType,
                ImagePath = vehicle.ImagePath,
                Notes = vehicle.Notes,
                CreatedAt = vehicle.CreatedAt,
                IsActive = vehicle.IsActive,
                TotalAppointments = vehicle.Appointments.Count,
                CompletedAppointments = vehicle.Appointments.Count(a => a.Status == AppointmentStatus.Completed),
                PendingAppointments = vehicle.Appointments.Count(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed),
                RecentAppointments = vehicle.Appointments
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(5)
                    .Select(a => new AppointmentDetailViewModel
                    {
                        Id = a.Id,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.Status,
                        ServiceTypeName = a.ServiceType?.Name ?? "N/A",
                        ServicePrice = a.ServiceType?.Price ?? 0
                    }).ToList()
            };

            ViewData["Title"] = $"Araç Detayı - {vehicle.VehicleInfo}";
            return View(viewModel);
        }

        // GET: Vehicle/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Yeni Araç Ekle";
            return View(new VehicleCreateViewModel());
        }

        // POST: Vehicle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate license plate
                if (await _vehicleService.IsLicensePlateExistsAsync(viewModel.LicensePlate))
                {
                    ModelState.AddModelError("LicensePlate", "Bu plaka sistemde zaten kayıtlı.");
                    ViewData["Title"] = "Yeni Araç Ekle";
                    return View(viewModel);
                }

                var userId = _userManager.GetUserId(User);

                var vehicle = new Vehicle
                {
                    LicensePlate = viewModel.LicensePlate.ToUpper(),
                    Brand = viewModel.Brand,
                    Model = viewModel.Model,
                    Year = viewModel.Year,
                    Color = viewModel.Color,
                    Mileage = viewModel.Mileage,
                    FuelType = viewModel.FuelType,
                    Notes = viewModel.Notes,
                    UserId = userId!
                };

                // Handle file upload
                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    try
                    {
                        vehicle.ImagePath = await _fileService.UploadFileAsync(viewModel.ImageFile, "vehicles");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        ViewData["Title"] = "Yeni Araç Ekle";
                        return View(viewModel);
                    }
                }

                await _vehicleService.CreateVehicleAsync(vehicle);
                TempData["Success"] = "Araç başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            // Collect model errors to help diagnose why it didn't save
            var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m));
            if (allErrors.Any())
            {
                TempData["Error"] = string.Join(" | ", allErrors);
            }
            ViewData["Title"] = "Yeni Araç Ekle";
            return View(viewModel);
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var vehicle = await _vehicleService.GetVehicleByIdAndUserAsync(id, userId!);

            if (vehicle == null)
            {
                TempData["Error"] = "Araç bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var model = new VehicleCreateViewModel
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType,
                Notes = vehicle.Notes,
                ExistingImagePath = vehicle.ImagePath
            };

            ViewData["Title"] = $"Araç Düzenle - {vehicle.VehicleInfo}";
            return View(model);
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleCreateViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var vehicle = await _vehicleService.GetVehicleByIdAndUserAsync(id, userId!);

                if (vehicle == null)
                {
                    TempData["Error"] = "Araç bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Check for duplicate license plate
                if (await _vehicleService.IsLicensePlateExistsAsync(viewModel.LicensePlate, id))
                {
                    ModelState.AddModelError("LicensePlate", "Bu plaka sistemde zaten kayıtlı.");
                    ViewData["Title"] = "Araç Düzenle";
                    return View(viewModel);
                }

                vehicle.LicensePlate = viewModel.LicensePlate.ToUpper();
                vehicle.Brand = viewModel.Brand;
                vehicle.Model = viewModel.Model;
                vehicle.Year = viewModel.Year;
                vehicle.Color = viewModel.Color;
                vehicle.Mileage = viewModel.Mileage;
                vehicle.FuelType = viewModel.FuelType;
                vehicle.Notes = viewModel.Notes;

                // Handle file upload
                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    try
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(vehicle.ImagePath))
                        {
                            await _fileService.DeleteFileAsync(vehicle.ImagePath);
                        }

                        vehicle.ImagePath = await _fileService.UploadFileAsync(viewModel.ImageFile, "vehicles");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        ViewData["Title"] = "Araç Düzenle";
                        return View(viewModel);
                    }
                }

                await _vehicleService.UpdateVehicleAsync(vehicle);
                TempData["Success"] = "Araç başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Araç Düzenle";
            return View(viewModel);
        }

        // POST: Vehicle/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var vehicle = await _vehicleService.GetVehicleByIdAndUserAsync(id, userId!);

            if (vehicle == null)
            {
                TempData["Error"] = "Araç bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Check for pending appointments
            var hasPendingAppointments = vehicle.Appointments.Any(a => 
                a.Status == AppointmentStatus.Pending || 
                a.Status == AppointmentStatus.Confirmed ||
                a.Status == AppointmentStatus.InProgress);

            if (hasPendingAppointments)
            {
                TempData["Error"] = "Bu araca ait bekleyen randevular var. Önce randevuları iptal edin.";
                return RedirectToAction(nameof(Index));
            }

            await _vehicleService.DeleteVehicleAsync(id);
            TempData["Success"] = "Araç başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
