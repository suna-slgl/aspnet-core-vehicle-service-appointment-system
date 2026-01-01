using Microsoft.AspNetCore.Razor.TagHelpers;
using VehicleServiceApp.Models;

namespace VehicleServiceApp.TagHelpers
{
    /// <summary>
    /// Custom Tag Helper - Appointment Status Badge
    /// Usage: <appointment-status status="@Model.Status"></appointment-status>
    /// </summary>
    [HtmlTargetElement("appointment-status")]
    public class AppointmentStatusTagHelper : TagHelper
    {
        [HtmlAttributeName("status")]
        public AppointmentStatus Status { get; set; }

        [HtmlAttributeName("show-icon")]
        public bool ShowIcon { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            var (colorClass, iconClass, text) = GetStatusInfo(Status);

            output.Attributes.SetAttribute("class", $"badge bg-{colorClass}");

            var icon = ShowIcon ? $"<i class=\"bi {iconClass} me-1\"></i>" : "";
            output.Content.SetHtmlContent($"{icon}{text}");
        }

        private static (string color, string icon, string text) GetStatusInfo(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Pending => ("warning", "bi-clock", "Beklemede"),
                AppointmentStatus.Confirmed => ("primary", "bi-check-circle", "Onaylandı"),
                AppointmentStatus.InProgress => ("info", "bi-gear", "Devam Ediyor"),
                AppointmentStatus.Completed => ("success", "bi-check-all", "Tamamlandı"),
                AppointmentStatus.Cancelled => ("danger", "bi-x-circle", "İptal Edildi"),
                _ => ("secondary", "bi-question-circle", "Bilinmiyor")
            };
        }
    }

    /// <summary>
    /// Custom Tag Helper - Price Display
    /// Usage: <price value="@Model.Price"></price>
    /// </summary>
    [HtmlTargetElement("price")]
    public class PriceTagHelper : TagHelper
    {
        [HtmlAttributeName("value")]
        public decimal Value { get; set; }

        [HtmlAttributeName("currency")]
        public string Currency { get; set; } = "₺";

        [HtmlAttributeName("css-class")]
        public string? CssClass { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            var cssClass = string.IsNullOrEmpty(CssClass) ? "text-success fw-bold" : CssClass;
            output.Attributes.SetAttribute("class", cssClass);

            output.Content.SetHtmlContent($"{Value:N2} {Currency}");
        }
    }

    /// <summary>
    /// Custom Tag Helper - Date Time Display
    /// Usage: <datetime value="@Model.DateTime" format="long"></datetime>
    /// </summary>
    [HtmlTargetElement("datetime")]
    public class DateTimeTagHelper : TagHelper
    {
        [HtmlAttributeName("value")]
        public DateTime Value { get; set; }

        [HtmlAttributeName("format")]
        public string Format { get; set; } = "short";

        [HtmlAttributeName("show-time")]
        public bool ShowTime { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            var formatted = Format.ToLower() switch
            {
                "long" => Value.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                "short" => Value.ToString("dd.MM.yyyy"),
                "relative" => GetRelativeTime(Value),
                _ => Value.ToString("dd.MM.yyyy")
            };

            if (ShowTime && Format.ToLower() != "relative")
            {
                formatted += " " + Value.ToString("HH:mm");
            }

            output.Attributes.SetAttribute("title", Value.ToString("dd.MM.yyyy HH:mm:ss"));
            output.Content.SetContent(formatted);
        }

        private static string GetRelativeTime(DateTime dateTime)
        {
            var diff = DateTime.Now - dateTime;

            if (diff.TotalMinutes < 1) return "Az önce";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dakika önce";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} saat önce";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} gün önce";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} hafta önce";
            if (diff.TotalDays < 365) return $"{(int)(diff.TotalDays / 30)} ay önce";
            return $"{(int)(diff.TotalDays / 365)} yıl önce";
        }
    }

    /// <summary>
    /// Custom Tag Helper - Confirm Button
    /// Usage: <confirm-button message="Silmek istediğinize emin misiniz?">Sil</confirm-button>
    /// </summary>
    [HtmlTargetElement("confirm-button")]
    public class ConfirmButtonTagHelper : TagHelper
    {
        [HtmlAttributeName("message")]
        public string Message { get; set; } = "Bu işlemi yapmak istediğinize emin misiniz?";

        [HtmlAttributeName("button-class")]
        public string ButtonClass { get; set; } = "btn btn-danger";

        [HtmlAttributeName("icon")]
        public string? Icon { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "button";
            output.Attributes.SetAttribute("type", "submit");
            output.Attributes.SetAttribute("class", ButtonClass);
            output.Attributes.SetAttribute("onclick", $"return confirm('{Message}')");

            var iconHtml = !string.IsNullOrEmpty(Icon) ? $"<i class=\"bi {Icon} me-1\"></i>" : "";
            output.PreContent.SetHtmlContent(iconHtml);
        }
    }

    /// <summary>
    /// Custom Tag Helper - Alert Messages
    /// Usage: <alert type="success" dismissible="true">Message here</alert>
    /// </summary>
    [HtmlTargetElement("alert")]
    public class AlertTagHelper : TagHelper
    {
        [HtmlAttributeName("type")]
        public string Type { get; set; } = "info";

        [HtmlAttributeName("dismissible")]
        public bool Dismissible { get; set; } = false;

        [HtmlAttributeName("icon")]
        public string? Icon { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            var alertClass = $"alert alert-{Type}";
            if (Dismissible)
            {
                alertClass += " alert-dismissible fade show";
            }

            output.Attributes.SetAttribute("class", alertClass);
            output.Attributes.SetAttribute("role", "alert");

            // Add icon if specified
            if (!string.IsNullOrEmpty(Icon))
            {
                output.PreContent.SetHtmlContent($"<i class=\"bi {Icon} me-2\"></i>");
            }

            // Add dismiss button if dismissible
            if (Dismissible)
            {
                output.PostContent.SetHtmlContent("<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>");
            }
        }
    }

    /// <summary>
    /// Custom Tag Helper - User Avatar
    /// Usage: <user-avatar image="@Model.ImagePath" name="@Model.FullName" size="40"></user-avatar>
    /// </summary>
    [HtmlTargetElement("user-avatar")]
    public class UserAvatarTagHelper : TagHelper
    {
        [HtmlAttributeName("image")]
        public string? ImagePath { get; set; }

        [HtmlAttributeName("name")]
        public string Name { get; set; } = "User";

        [HtmlAttributeName("size")]
        public int Size { get; set; } = 40;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "d-inline-block");
            output.Attributes.SetAttribute("title", Name);

            if (!string.IsNullOrEmpty(ImagePath))
            {
                output.Content.SetHtmlContent(
                    $"<img src=\"{ImagePath}\" alt=\"{Name}\" class=\"rounded-circle\" width=\"{Size}\" height=\"{Size}\" style=\"object-fit: cover;\">");
            }
            else
            {
                // Generate avatar with initials
                var initials = GetInitials(Name);
                var bgColor = GetColorFromName(Name);
                output.Content.SetHtmlContent(
                    $"<div class=\"rounded-circle d-flex align-items-center justify-content-center text-white fw-bold\" " +
                    $"style=\"width: {Size}px; height: {Size}px; background-color: {bgColor}; font-size: {Size * 0.4}px;\">{initials}</div>");
            }
        }

        private static string GetInitials(string name)
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return parts.Length > 0 ? parts[0][0].ToString().ToUpper() : "?";
        }

        private static string GetColorFromName(string name)
        {
            var colors = new[] { "#3498db", "#e74c3c", "#2ecc71", "#9b59b6", "#f39c12", "#1abc9c", "#34495e" };
            var hash = name.GetHashCode();
            return colors[Math.Abs(hash) % colors.Length];
        }
    }
}
