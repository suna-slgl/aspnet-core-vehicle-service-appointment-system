using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Data;
using VehicleServiceApp.Filters;
using VehicleServiceApp.Middleware;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services;
using VehicleServiceApp.Services.Interfaces;
using System.Globalization;
using System.Text;

// Set encoding for Turkish characters
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Configure UTF-8 encoding globally
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

// Configure Turkish culture
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("tr-TR") };
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity with custom ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Register Services with different lifetimes
// Scoped - per request
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IServiceTypeService, ServiceTypeService>();
builder.Services.AddScoped<ITechnicianService, TechnicianService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Transient - new instance each time
builder.Services.AddTransient<IFileService, FileService>();

// Register Filters
builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddScoped<AdminActionFilter>();
builder.Services.AddScoped<PerformanceFilter>();
builder.Services.AddScoped<CustomExceptionFilter>();
builder.Services.AddScoped<CommonViewDataFilter>();

// Add MVC with filters
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CommonViewDataFilter>();
});

// Add Razor Pages for Identity UI
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Custom Middleware
app.UseSecurityHeaders();
app.UseRequestLogging();

// Request Localization for Turkish
app.UseRequestLocalization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Area routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default routing (Conventional Routing)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply migrations
        context.Database.Migrate();

        // Seed Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Seed Admin User
        var adminEmail = "admin@aracservis.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed Demo User
        var demoEmail = "demo@aracservis.com";
        if (await userManager.FindByEmailAsync(demoEmail) == null)
        {
            var demoUser = new ApplicationUser
            {
                UserName = demoEmail,
                Email = demoEmail,
                FirstName = "Demo",
                LastName = "Kullanıcı",
                PhoneNumber = "0532 123 45 67",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(demoUser, "Demo123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(demoUser, "User");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
