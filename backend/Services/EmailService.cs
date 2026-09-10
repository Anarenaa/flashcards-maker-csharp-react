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

        public async Task SendEmailAsync(string toEmail, string subject, string body, string? replyToEmail = null)
        {
            var from = _configuration["EmailSettings:From"];
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var SmtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            var fromAddress = new MailAddress(from!, "Flashcards Maker");
            var toAddress = new MailAddress(toEmail);

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            if (!string.IsNullOrEmpty(replyToEmail))
            {
                message.ReplyToList.Add(new MailAddress(replyToEmail));
            }

            using var client = new SmtpClient(smtpServer, SmtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}
