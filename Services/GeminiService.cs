using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DTOs;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;

        public GeminiService(HttpClient client, string apiKey)
        {
            _client = client;
            _apiKey = apiKey;
        }
        public async Task<List<FlashcardDTO>> GenerateCardsAsync(string prompt, byte[]? imageBytes = null, string? mimeType = null)
        {
            _client.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);
            var url = $"interactions";

            object inputData;
            if (imageBytes != null && !string.IsNullOrEmpty(mimeType))
            {
                inputData = new object[]
                {
                    new
                    {
                        type = "text",
                        text = $"Generate {prompt}. Format ONLY as a JSON array: [{{'Term': '...', 'Definition': '...'}}]."
                    },
                    new
                    {
                        type = "image",
                        data = Convert.ToBase64String(imageBytes),
                        mime_type = mimeType
                    }
                };
            }
            else
            {
                inputData = $"Generate {prompt}. Format ONLY as a JSON array: [{{'Term': '...', 'Definition': '...'}}].";
            }

            var requestBody = new
            {
                model = "gemini-3-flash-preview",
                input = inputData
            };

            var response = await _client.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

            // В Interactions API текст лежить в Outputs, а не в Candidates
            var rawJson = result?.Outputs?.LastOrDefault()?.Text;

            if (string.IsNullOrEmpty(rawJson)) return new List<FlashcardDTO>();

            if (rawJson.Contains("```"))
            {
                rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
            }

            return JsonSerializer.Deserialize<List<FlashcardDTO>>(rawJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<FlashcardDTO>();
        }
    }
    public class GeminiResponse
    {
        [JsonPropertyName("outputs")]
        public List<GeminiOutput> Outputs { get; set; } = new();
    }

    public class GeminiOutput
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
