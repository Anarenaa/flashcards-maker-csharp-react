using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using IDictionaryService = Services.Interfaces.IDictionaryService;

namespace Services
{
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;
        public DictionaryService(HttpClient httpClient, ILogger<DictionaryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> GetTranslationAsync(string term, string fromLang, string toLang)
        {
            var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(term)}&langpair={fromLang}|{toLang}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            try
            {
                var data = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (data.TryGetProperty("responseData", out var responseData) &&
                    responseData.TryGetProperty("translatedText", out var translatedText))
                {
                    var result = translatedText.GetString();
                    return string.IsNullOrWhiteSpace(result) ? null : result;
                }

                _logger.LogWarning("MyMemory API response structure is invalid or missing properties for term: '{Term}'", term);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching translation for term: '{Term}' from {From} to {To}", term, fromLang, toLang);
                return null;
            }
        }
    }
}
