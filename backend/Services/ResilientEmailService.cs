using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    /// <summary>
    /// Decorator over any IEmailService: aborts waiting after a timeout and
    /// opens a circuit breaker after repeated failures, so a hung SMTP server
    /// can never block an HTTP request.
    /// </summary>
    public class ResilientEmailService : IEmailService
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(60);
        private const int FailureThreshold = 3;

        private static readonly object Lock = new();
        private static int _failures;
        private static DateTime _openUntilUtc = DateTime.MinValue;

        private readonly IEmailService _inner;
        private readonly ILogger<ResilientEmailService> _logger;

        public ResilientEmailService(IEmailService inner, ILogger<ResilientEmailService> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, string? replyToEmail = null)
        {
            lock (Lock)
            {
                if (DateTime.UtcNow < _openUntilUtc)
                    throw new EmailDeliveryException("Email service is temporarily unavailable.");
            }

            try
            {
                await _inner.SendEmailAsync(toEmail, subject, body, replyToEmail).WaitAsync(Timeout);
                lock (Lock) { _failures = 0; }
            }
            catch (Exception ex)
            {
                lock (Lock)
                {
                    if (++_failures >= FailureThreshold)
                    {
                        _openUntilUtc = DateTime.UtcNow + Cooldown;
                        _failures = 0;
                    }
                }
                _logger.LogError(ex, "Sending email failed or timed out");
                throw new EmailDeliveryException("Email sending failed.", ex);
            }
        }
    }

    public class EmailDeliveryException : Exception
    {
        public EmailDeliveryException(string message, Exception? inner = null) : base(message, inner) { }
    }
}