using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.Controllers
{
    /// <summary>
    /// Account Controller - Handles user authentication and profile management
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IFileService _fileService;
        private readonly IVehicleService _vehicleService;
        private readonly IAppointmentService _appointmentService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileService fileService,
            IVehicleService vehicleService,
            IAppointmentService appointmentService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileService = fileService;
            _vehicleService = vehicleService;
            _appointmentService = appointmentService;
        }

        // GET: Account/Register
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Kayıt Ol";
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                // Handle profile image upload
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    try
                    {
                        user.ProfileImagePath = await _fileService.UploadFileAsync(model.ProfileImage, "profiles");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("ProfileImage", ex.Message);
                        ViewData["Title"] = "Kayıt Ol";
                        return View(model);
                    }
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Add to User role
                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["Success"] = "Hoş geldiniz! Hesabınız başarıyla oluşturuldu.";
                    return await RedirectAfterSignIn(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["Title"] = "Kayıt Ol";
            return View(model);
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null, bool force = false)
        {
            if (!force && User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Giriş Yap";
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Hesabınız devre dışı bırakılmış.");
                    ViewData["Title"] = "Giriş Yap";
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Hoş geldiniz!";
                    return await RedirectAfterSignIn(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Hesabınız geçici olarak kilitlendi. Lütfen birkaç dakika sonra tekrar deneyin.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz email veya şifre.");
                }
            }

            ViewData["Title"] = "Giriş Yap";
            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Güvenli bir şekilde çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ExistingProfileImagePath = user.ProfileImagePath,
                CreatedAt = user.CreatedAt,
                TotalVehicles = await _vehicleService.GetVehicleCountByUserAsync(user.Id),
                TotalAppointments = await _appointmentService.GetAppointmentCountByUserAsync(user.Id),
                CompletedAppointments = await _appointmentService.GetCompletedAppointmentCountByUserAsync(user.Id)
            };

            ViewData["Title"] = "Profilim";
            return View(model);
        }

        // POST: Account/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.UpdatedAt = DateTime.Now;

                // Handle profile image upload
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    try
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(user.ProfileImagePath))
                        {
                            await _fileService.DeleteFileAsync(user.ProfileImagePath);
                        }

                        user.ProfileImagePath = await _fileService.UploadFileAsync(model.ProfileImage, "profiles");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("ProfileImage", ex.Message);
                        ViewData["Title"] = "Profilim";
                        return View(model);
                    }
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Profil bilgileriniz güncellendi.";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.ExistingProfileImagePath = user.ProfileImagePath;
            model.TotalVehicles = await _vehicleService.GetVehicleCountByUserAsync(user.Id);
            model.TotalAppointments = await _appointmentService.GetAppointmentCountByUserAsync(user.Id);
            model.CompletedAppointments = await _appointmentService.GetCompletedAppointmentCountByUserAsync(user.Id);

            ViewData["Title"] = "Profilim";
            return View(model);
        }

        // GET: Account/ChangePassword
        [Authorize]
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Şifre Değiştir";
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["Title"] = "Şifre Değiştir";
            return View(model);
        }

        // GET: Account/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Erişim Engellendi";
            return View();
        }

        private async Task<IActionResult> RedirectAfterSignIn(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
