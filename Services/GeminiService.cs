using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _client;
        public GeminiService(HttpClient client, ILogger<GeminiService> logger)
        {
            _client = client;
        }
    }
}
