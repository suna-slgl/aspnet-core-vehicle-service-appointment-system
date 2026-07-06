using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Services
{
    /// <summary>
    /// Default email service. Replace this implementation with SMTP-backed delivery in production.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email address is required.", nameof(to));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Email subject is required.", nameof(subject));

            _logger.LogInformation("Email queued for {To}. Subject: {Subject}. Body: {Body}", to, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}
