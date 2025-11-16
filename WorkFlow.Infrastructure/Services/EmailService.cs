using WorkFlow.Application.Common.Interfaces.Services;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

namespace WorkFlow.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email cannot be empty.", nameof(to));

            var fromAddress = _config["EmailSettings:From"];
            var host = _config["EmailSettings:Host"];
            var portString = _config["EmailSettings:Port"];
            var username = _config["EmailSettings:Username"];

            if (string.IsNullOrWhiteSpace(fromAddress) || 
                string.IsNullOrWhiteSpace(host) || 
                string.IsNullOrWhiteSpace(portString) || 
                string.IsNullOrWhiteSpace(username))
            {
                throw new InvalidOperationException("Email settings are not properly configured in appsettings.json.");
            }

            if (!int.TryParse(portString, out var port))
                throw new InvalidOperationException("EmailSettings:Port must be a valid number.");
            var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") 
                           ?? _config["EmailSettings:Password"];

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Email password is not set. Please configure environment variable EMAIL_PASSWORD or appsettings.json.");

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(fromAddress));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
