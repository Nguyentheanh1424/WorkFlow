using Microsoft.Extensions.Logging;
using WorkFlow.Application.Common.Interfaces.Services;

namespace WorkFlow.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            // Giả lập việc gửi email
            _logger.LogInformation($"Email sent to {to} - Subject: {subject}");
            return Task.CompletedTask;
        }
    }
}
