using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VehicleServiceApp.Filters
{
    /// <summary>
    /// Custom Action Filter - Logs action execution
    /// </summary>
    public class LogActionFilter : IActionFilter
    {
        private readonly ILogger<LogActionFilter> _logger;

        public LogActionFilter(ILogger<LogActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation(
                "Action executing: {Controller}.{Action}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"]);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                _logger.LogError(
                    context.Exception,
                    "Action error: {Controller}.{Action}",
                    context.RouteData.Values["controller"],
                    context.RouteData.Values["action"]);
            }
            else
            {
                _logger.LogInformation(
                    "Action executed: {Controller}.{Action} - Status: {Status}",
                    context.RouteData.Values["controller"],
                    context.RouteData.Values["action"],
                    context.HttpContext.Response.StatusCode);
            }
        }
    }

    /// <summary>
    /// Custom Action Filter - Admin area authorization and logging
    /// </summary>
    public class AdminActionFilter : IActionFilter
    {
        private readonly ILogger<AdminActionFilter> _logger;

        public AdminActionFilter(ILogger<AdminActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            var userName = user.Identity?.Name ?? "Anonymous";

            _logger.LogInformation(
                "Admin action: {User} accessing {Controller}.{Action}",
                userName,
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"]);

            // Store admin action info for audit
            context.HttpContext.Items["AdminActionTime"] = DateTime.Now;
            context.HttpContext.Items["AdminUser"] = userName;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var startTime = context.HttpContext.Items["AdminActionTime"] as DateTime?;
            if (startTime.HasValue)
            {
                var duration = DateTime.Now - startTime.Value;
                _logger.LogInformation(
                    "Admin action completed in {Duration}ms",
                    duration.TotalMilliseconds);
            }
        }
    }

    /// <summary>
    /// Custom Result Filter - Adds common ViewData
    /// </summary>
    public class CommonViewDataFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Controller is Controller controller)
            {
                controller.ViewData["AppName"] = "Araç Servis Randevu Sistemi";
                controller.ViewData["CurrentYear"] = DateTime.Now.Year;
                controller.ViewData["Version"] = "1.0.0";
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            // No action needed after result
        }
    }

    /// <summary>
    /// Custom Async Action Filter - Performance monitoring
    /// </summary>
    public class PerformanceFilter : IAsyncActionFilter
    {
        private readonly ILogger<PerformanceFilter> _logger;
        private const int WarningThresholdMs = 500;

        public PerformanceFilter(ILogger<PerformanceFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var result = await next();

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > WarningThresholdMs)
            {
                _logger.LogWarning(
                    "Slow action detected: {Controller}.{Action} took {Duration}ms",
                    context.RouteData.Values["controller"],
                    context.RouteData.Values["action"],
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }

    /// <summary>
    /// Custom Exception Filter - Handles specific exceptions
    /// </summary>
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilter> _logger;
        private readonly IHostEnvironment _environment;

        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(
                context.Exception,
                "Unhandled exception in {Controller}.{Action}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"]);

            // In development, let the exception propagate
            // In production, you might want to handle it differently
            if (!_environment.IsDevelopment())
            {
                context.ExceptionHandled = true;
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Error", "Home", null);
            }
        }
    }
}
