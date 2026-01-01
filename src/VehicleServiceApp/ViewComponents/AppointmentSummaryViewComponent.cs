using Microsoft.AspNetCore.Mvc;
using VehicleServiceApp.Services.Interfaces;
using VehicleServiceApp.ViewModels;

namespace VehicleServiceApp.ViewComponents
{
    /// <summary>
    /// View Component - Appointment Summary for Dashboard
    /// Usage: @await Component.InvokeAsync("AppointmentSummary")
    /// </summary>
    public class AppointmentSummaryViewComponent : ViewComponent
    {
        private readonly IDashboardService _dashboardService;

        public AppointmentSummaryViewComponent(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync();
            
            var summary = new AppointmentSummaryViewModel
            {
                TodayAppointments = dashboard.TodayAppointments,
                PendingAppointments = dashboard.PendingAppointments,
                CompletedToday = dashboard.TodayCompletedAppointments,
                UpcomingAppointments = dashboard.UpcomingTodayAppointments
            };

            return View(summary);
        }
    }

    /// <summary>
    /// ViewModel for Appointment Summary View Component
    /// </summary>
    public class AppointmentSummaryViewModel
    {
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedToday { get; set; }
        public List<AppointmentDetailViewModel> UpcomingAppointments { get; set; } = new();
    }
}
