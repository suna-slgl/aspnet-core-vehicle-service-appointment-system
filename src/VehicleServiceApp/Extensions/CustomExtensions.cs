using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleServiceApp.Models;

namespace VehicleServiceApp.Extensions
{
    /// <summary>
    /// Custom HTML Helpers
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders an alert message if TempData contains it
        /// </summary>
        public static IHtmlContent RenderAlerts(this IHtmlHelper htmlHelper)
        {
            var content = new HtmlContentBuilder();

            var tempData = htmlHelper.ViewContext.TempData;

            if (tempData["Success"] != null)
            {
                content.AppendHtml(CreateAlert("success", tempData["Success"]!.ToString()!, "bi-check-circle"));
            }
            if (tempData["Error"] != null)
            {
                content.AppendHtml(CreateAlert("danger", tempData["Error"]!.ToString()!, "bi-exclamation-circle"));
            }
            if (tempData["Warning"] != null)
            {
                content.AppendHtml(CreateAlert("warning", tempData["Warning"]!.ToString()!, "bi-exclamation-triangle"));
            }
            if (tempData["Info"] != null)
            {
                content.AppendHtml(CreateAlert("info", tempData["Info"]!.ToString()!, "bi-info-circle"));
            }

            return content;
        }

        private static string CreateAlert(string type, string message, string icon)
        {
            return $@"
                <div class='alert alert-{type} alert-dismissible fade show' role='alert'>
                    <i class='bi {icon} me-2'></i>{message}
                    <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
                </div>";
        }

        /// <summary>
        /// Renders a loading spinner
        /// </summary>
        public static IHtmlContent LoadingSpinner(this IHtmlHelper htmlHelper, string text = "Yükleniyor...", string size = "sm")
        {
            var sizeClass = size == "lg" ? "" : "spinner-border-sm";
            return new HtmlString($@"
                <div class='text-center'>
                    <div class='spinner-border {sizeClass}' role='status'>
                        <span class='visually-hidden'>{text}</span>
                    </div>
                    <span class='ms-2'>{text}</span>
                </div>");
        }

        /// <summary>
        /// Renders an empty state message
        /// </summary>
        public static IHtmlContent EmptyState(this IHtmlHelper htmlHelper, string message, string icon = "bi-inbox", string? actionUrl = null, string? actionText = null)
        {
            var actionButton = !string.IsNullOrEmpty(actionUrl) && !string.IsNullOrEmpty(actionText)
                ? $"<a href='{actionUrl}' class='btn btn-primary mt-3'>{actionText}</a>"
                : "";

            return new HtmlString($@"
                <div class='text-center py-5'>
                    <i class='bi {icon} display-1 text-muted'></i>
                    <p class='lead mt-3 text-muted'>{message}</p>
                    {actionButton}
                </div>");
        }

        /// <summary>
        /// Renders pagination controls
        /// </summary>
        public static IHtmlContent Pagination(this IHtmlHelper htmlHelper, int currentPage, int totalPages, string actionName, string? controllerName = null)
        {
            if (totalPages <= 1) return HtmlString.Empty;

            var builder = new HtmlContentBuilder();
            builder.AppendHtml("<nav><ul class='pagination justify-content-center'>");

            // Previous button
            var prevDisabled = currentPage == 1 ? "disabled" : "";
            var prevPage = Math.Max(1, currentPage - 1);
            builder.AppendHtml($"<li class='page-item {prevDisabled}'><a class='page-link' href='?page={prevPage}'>&laquo;</a></li>");

            // Page numbers
            var startPage = Math.Max(1, currentPage - 2);
            var endPage = Math.Min(totalPages, currentPage + 2);

            if (startPage > 1)
            {
                builder.AppendHtml("<li class='page-item'><a class='page-link' href='?page=1'>1</a></li>");
                if (startPage > 2)
                    builder.AppendHtml("<li class='page-item disabled'><span class='page-link'>...</span></li>");
            }

            for (int i = startPage; i <= endPage; i++)
            {
                var active = i == currentPage ? "active" : "";
                builder.AppendHtml($"<li class='page-item {active}'><a class='page-link' href='?page={i}'>{i}</a></li>");
            }

            if (endPage < totalPages)
            {
                if (endPage < totalPages - 1)
                    builder.AppendHtml("<li class='page-item disabled'><span class='page-link'>...</span></li>");
                builder.AppendHtml($"<li class='page-item'><a class='page-link' href='?page={totalPages}'>{totalPages}</a></li>");
            }

            // Next button
            var nextDisabled = currentPage == totalPages ? "disabled" : "";
            var nextPage = Math.Min(totalPages, currentPage + 1);
            builder.AppendHtml($"<li class='page-item {nextDisabled}'><a class='page-link' href='?page={nextPage}'>&raquo;</a></li>");

            builder.AppendHtml("</ul></nav>");

            return builder;
        }

        /// <summary>
        /// Renders a card component
        /// </summary>
        public static IHtmlContent Card(this IHtmlHelper htmlHelper, string title, string content, string? icon = null, string? headerClass = null)
        {
            var iconHtml = !string.IsNullOrEmpty(icon) ? $"<i class='bi {icon} me-2'></i>" : "";
            var headerCssClass = !string.IsNullOrEmpty(headerClass) ? headerClass : "bg-primary text-white";

            return new HtmlString($@"
                <div class='card shadow-sm'>
                    <div class='card-header {headerCssClass}'>
                        {iconHtml}{title}
                    </div>
                    <div class='card-body'>
                        {content}
                    </div>
                </div>");
        }
    }

    /// <summary>
    /// LINQ Extension Methods
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Paginates a queryable collection
        /// </summary>
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Paginates an enumerable collection
        /// </summary>
        public static IEnumerable<T> Paginate<T>(this IEnumerable<T> collection, int pageNumber, int pageSize)
        {
            return collection.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Filters by date range
        /// </summary>
        public static IEnumerable<Appointment> FilterByDateRange(this IEnumerable<Appointment> appointments, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate >= startDate.Value.Date);

            if (endDate.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate <= endDate.Value.Date);

            return appointments;
        }

        /// <summary>
        /// Filters by status
        /// </summary>
        public static IEnumerable<Appointment> FilterByStatus(this IEnumerable<Appointment> appointments, AppointmentStatus? status)
        {
            if (status.HasValue)
                return appointments.Where(a => a.Status == status.Value);

            return appointments;
        }
    }

    /// <summary>
    /// String Extension Methods
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Truncates a string to specified length with ellipsis
        /// </summary>
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..(maxLength - suffix.Length)] + suffix;
        }

        /// <summary>
        /// Converts string to title case
        /// </summary>
        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Formats license plate (Turkish format)
        /// </summary>
        public static string FormatLicensePlate(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var cleaned = value.Replace(" ", "").ToUpper();
            if (cleaned.Length >= 7)
            {
                // Format: XX YYY NNNN
                var cityCode = cleaned[..2];
                var letters = "";
                var numbers = "";
                
                for (int i = 2; i < cleaned.Length; i++)
                {
                    if (char.IsLetter(cleaned[i]))
                        letters += cleaned[i];
                    else
                        numbers += cleaned[i];
                }
                
                return $"{cityCode} {letters} {numbers}";
            }
            return cleaned;
        }
    }

    /// <summary>
    /// DateTime Extension Methods
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns Turkish day name
        /// </summary>
        public static string ToTurkishDayName(this DateTime date)
        {
            return date.ToString("dddd", new System.Globalization.CultureInfo("tr-TR"));
        }

        /// <summary>
        /// Returns Turkish month name
        /// </summary>
        public static string ToTurkishMonthName(this DateTime date)
        {
            return date.ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));
        }

        /// <summary>
        /// Returns formatted date in Turkish
        /// </summary>
        public static string ToTurkishDate(this DateTime date, bool includeTime = false)
        {
            var format = includeTime ? "dd MMMM yyyy HH:mm" : "dd MMMM yyyy";
            return date.ToString(format, new System.Globalization.CultureInfo("tr-TR"));
        }

        /// <summary>
        /// Gets start of week (Monday)
        /// </summary>
        public static DateTime StartOfWeek(this DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        /// <summary>
        /// Gets end of week (Sunday)
        /// </summary>
        public static DateTime EndOfWeek(this DateTime date)
        {
            return date.StartOfWeek().AddDays(6);
        }

        /// <summary>
        /// Gets relative time string
        /// </summary>
        public static string ToRelativeTime(this DateTime date)
        {
            var diff = DateTime.Now - date;

            if (diff.TotalSeconds < 60) return "Az önce";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dakika önce";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} saat önce";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} gün önce";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} hafta önce";
            if (diff.TotalDays < 365) return $"{(int)(diff.TotalDays / 30)} ay önce";
            return $"{(int)(diff.TotalDays / 365)} yıl önce";
        }
    }
}
