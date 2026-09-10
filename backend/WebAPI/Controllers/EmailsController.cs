using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailsController : BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public EmailsController(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("to-support")]
        public async Task<IActionResult> SendEmailToSupport([FromBody] ContactDto model)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name);

            var supportEmail = _configuration["EmailSettings:From"];
            var subject = $"[Підтримка] {model.Subject} — від {userName}";

            var body = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #f9f9f9;"">
                    <h2 style=""color: #333; border-bottom: 2px solid #007bff; padding-bottom: 8px; margin-top: 0;"">💬 Нове звернення до підтримки</h2>
  
                    <p style=""margin: 10px 0;""><strong>Користувач:</strong> {userName} (<a href=""mailto:{userEmail}"" style=""color: #007bff; text-decoration: none;"">{userEmail}</a>)</p>
                    <p style=""margin: 10px 0;""><strong>Тема:</strong> {model.Subject}</p>
  
                    <div style=""background-color: #ffffff; padding: 15px; border-radius: 6px; border: 1px solid #eaeaea; margin-top: 15px;"">
                    <p style=""margin: 0 0 5px 0; color: #666; font-size: 12px; text-transform: uppercase;""><strong>Повідомлення:</strong></p>
                    <p style=""margin: 0; color: #222; white-space: pre-wrap; line-height: 1.5;"">{model.Message}</p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(supportEmail!, subject, body, userEmail);

            return Ok(new { message = "Лист успішно надіслано!" });
        }
    }

    public class ContactDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
