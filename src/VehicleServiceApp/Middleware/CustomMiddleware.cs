namespace VehicleServiceApp.Middleware
{
    /// <summary>
    /// Custom Middleware - Exception Handler Middleware
    /// Logs exceptions and redirects to error page
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. Path: {Path}", context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // AJAX request - return JSON
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                    details = exception.Message
                });
            }
            else
            {
                // Regular request - redirect to error page
                context.Response.Redirect("/Home/Error");
            }
        }
    }

    /// <summary>
    /// Custom Middleware - Request Logging Middleware
    /// Logs all incoming requests
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var request = context.Request;

            _logger.LogInformation(
                "Request started: {Method} {Path} at {Time}",
                request.Method,
                request.Path,
                startTime.ToString("HH:mm:ss.fff"));

            await _next(context);

            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Custom Middleware - Security Headers Middleware
    /// Adds security headers to all responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            await _next(context);
        }
    }

    /// <summary>
    /// Extension methods for middleware registration
    /// </summary>
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlerMiddleware>();
        }

        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
