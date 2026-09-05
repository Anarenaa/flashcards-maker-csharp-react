using System.Net.Http.Json;
using System.Text.Json;
using Services.Interfaces;

namespace Services
{
    public class WikipediaService : IWikipediaService
    {
        private readonly HttpClient _httpClient;
        public WikipediaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetDefinitionAsync(string term, string lang)
        {
            try
            {
                var url = $"https://{lang}.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(term)}";
                // Wikipedia обов'язково вимагає User-Agent
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "FlashcardsApp/1.0");

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var data = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (data.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
                {
                    return desc.GetString();
                }

                if (data.TryGetProperty("extract", out var extract) && extract.ValueKind == JsonValueKind.String)
                {
                    return extract.GetString();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
