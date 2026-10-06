using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services
{
    /// <summary>
    /// Sends e-mail through the Brevo HTTPS API (port 443), which, unlike SMTP,
    /// is not blocked by hosting providers. Timeout/circuit breaker are provided
    /// by ResilientEmailService, which wraps this class.
    /// </summary>
    public class BrevoEmailService : IEmailService
    {
        private const string Endpoint = "https://api.brevo.com/v3/smtp/email";

        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public BrevoEmailService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, string? replyToEmail = null)
        {
            var from = _configuration["EmailSettings:From"];
            var apiKey = _configuration["EmailSettings:BrevoApiKey"];
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Brevo settings are not configured.");

            var payload = new Dictionary<string, object?>
            {
                ["sender"] = new { name = "Flashcards Maker", email = from },
                ["to"] = new[] { new { email = toEmail } },
                ["subject"] = subject,
                ["htmlContent"] = body
            };
            if (!string.IsNullOrEmpty(replyToEmail))
                payload["replyTo"] = new { email = replyToEmail };

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("api-key", apiKey);
            request.Headers.Add("accept", "application/json");

            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Brevo returned {(int)response.StatusCode}: {details}");
            }
        }
    }
}