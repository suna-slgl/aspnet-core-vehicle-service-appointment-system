using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Areas.Admin.Controllers
{
    /// <summary>
    /// User management controller for Admin Area
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private const int PageSize = 15;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVehicleService _vehicleService;
        private readonly IAppointmentService _appointmentService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            IVehicleService vehicleService,
            IAppointmentService appointmentService)
        {
            _userManager = userManager;
            _vehicleService = vehicleService;
            _appointmentService = appointmentService;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string? searchTerm, bool? isActive, int page = 1)
        {
            page = Math.Max(page, 1);

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    u.Email!.Contains(searchTerm) ||
                    u.FirstName.Contains(searchTerm) ||
                    u.LastName.Contains(searchTerm));
            }

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var model = new UserListViewModel
            {
                SearchTerm = searchTerm,
                IsActive = isActive,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = PageSize
            };

            foreach (var user in users)
            {
                model.Users.Add(await BuildUserManageViewModelAsync(user));
            }

            ViewData["Title"] = "Kullanıcılar";
            return View(model);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = await BuildUserManageViewModelAsync(user);
            model.Vehicles = (await _vehicleService.GetVehiclesByUserIdAsync(user.Id))
                .Select(v => new VehicleDetailViewModel
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
                    IsActive = v.IsActive
                })
                .ToList();

            model.RecentAppointments = (await _appointmentService.GetAppointmentsByUserIdAsync(user.Id))
                .Take(10)
                .Select(a => new AppointmentDetailViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status,
                    VehicleInfo = a.Vehicle?.VehicleInfo ?? "N/A",
                    ServiceTypeName = a.ServiceType?.Name ?? "N/A",
                    ServicePrice = a.ServiceType?.Price ?? 0,
                    TechnicianName = a.Technician?.FullName,
                    CustomerName = user.FullName,
                    CustomerEmail = user.Email ?? "N/A"
                })
                .ToList();

            ViewData["Title"] = "Kullanıcı Detayı";
            return View(model);
        }

        // POST: Admin/Users/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "Kendi hesabınızı bu ekrandan devre dışı bırakamazsınız.";
                return RedirectToAction(nameof(Index));
            }

            if (user.IsActive && await IsLastActiveAdminAsync(user))
            {
                TempData["Error"] = "Son aktif admin kullanıcısı devre dışı bırakılamaz.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                TempData["Success"] = user.IsActive ? "Kullanıcı aktifleştirildi." : "Kullanıcı devre dışı bırakıldı.";
            else
                TempData["Error"] = "Kullanıcı durumu güncellenemedi.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<UserManageViewModel> BuildUserManageViewModelAsync(ApplicationUser user)
        {
            return new UserManageViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                VehicleCount = await _vehicleService.GetVehicleCountByUserAsync(user.Id),
                AppointmentCount = await _appointmentService.GetAppointmentCountByUserAsync(user.Id)
            };
        }

        private async Task<bool> IsLastActiveAdminAsync(ApplicationUser user)
        {
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                return false;

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            return admins.Count(a => a.IsActive && a.Id != user.Id) == 0;
        }
    }
}
