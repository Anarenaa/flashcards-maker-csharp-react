using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services
{
    public class EmailService : IEmailService
    {
        private IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var from = _configuration["EmailSettings:From"];
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var SmtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            var message = new MailMessage(from!, toEmail, subject, body);
            message.IsBodyHtml = true;

            using var client = new SmtpClient(smtpServer, SmtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}
