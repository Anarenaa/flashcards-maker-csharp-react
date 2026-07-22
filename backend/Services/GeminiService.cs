using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DTOs;
using Core.Models;
using Ganss.Xss;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;
        private readonly ILogger<GeminiService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        private readonly JsonSerializerOptions _jsonOptions = new() 
        { 
            PropertyNameCaseInsensitive = true 
        };

        public GeminiService(HttpClient client, string apiKey, ILogger<GeminiService> logger, IUnitOfWork unitOfWork)
        {
            _client = client;
            _apiKey = apiKey;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<FlashcardDTO>> GenerateCardsAsync(SetCreateDTO setDto, int cardsCount, byte[]? imageBytes = null, string? mimeType = null)
        {
            var url = "interactions";

            object inputData;
            string text = $"Generate {setDto.Description}. Title \"{setDto.Name}\". " +
                          $"Type \"{setDto.Type.ToString()}\". If Type is Language generate cards from {setDto.FromLang} to {setDto.ToLang}. " +
                          $"{cardsCount} cards. Format ONLY as a JSON array: [{{'Term': '...', 'Definition': '...'}}].";
            
            if (imageBytes != null && !string.IsNullOrEmpty(mimeType))
            {
                inputData = new object[]
                {
                    new { type = "text", text = text },
                    new { type = "image", data = Convert.ToBase64String(imageBytes), mime_type = mimeType }
                };
            }
            else
            {
                inputData = text;
            }

            var requestBody = new
            {
                model = "gemini-2.5-flash",
                input = inputData
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("x-goog-api-key", _apiKey);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var rawTextFromGoogle = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw response from Gemini API: " + rawTextFromGoogle);

            var result = JsonSerializer.Deserialize<GeminiResponse>(rawTextFromGoogle, _jsonOptions);

            var outputStep = result?.Steps?.FirstOrDefault(s => s.Type == "model_output");
            var rawJson = outputStep?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;

            if (string.IsNullOrEmpty(rawJson)) return new List<FlashcardDTO>();

            if (rawJson.Contains("```"))
            {
                rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
            }

            return JsonSerializer.Deserialize<List<FlashcardDTO>>(rawJson, _jsonOptions) ?? new List<FlashcardDTO>();
        }

        public async Task<string?> GenerateSimpleHintAsync(string term, string lang, SetType type)
        {
            var url = "interactions";

            string prompt = type == SetType.Language
                ? $"Translate the word '{term}' to language with ISO code '{lang}'. Return ONLY the translated word/phrase, no extra text, no explanations, no quotes."
                : $"Give a one-sentence definition of the term '{term}' in language with ISO code '{lang}'. Max 15 words. Return ONLY the definition text, no extra words.";

            var requestBody = new
            {
                model = "gemini-2.5-flash",
                input = prompt
            };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(requestBody)
                };
                request.Headers.Add("x-goog-api-key", _apiKey);

                var response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini API returned status code: {StatusCode} during simple hint generation for '{Term}'", response.StatusCode, term);
                    return null;
                }

                var rawTextFromGoogle = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Raw response from Gemini API: " + rawTextFromGoogle);

                var result = JsonSerializer.Deserialize<GeminiResponse>(rawTextFromGoogle, _jsonOptions);
                
                var outputStep = result?.Steps?.FirstOrDefault(s => s.Type == "model_output");
                var rawJson = outputStep?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;

                if (string.IsNullOrWhiteSpace(rawJson)) return null;

                var cleanText = rawJson.Trim().Trim('"', '\'', '`', '\n', '\r');

                return string.IsNullOrWhiteSpace(cleanText) ? null : cleanText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating Gemini hint for term: '{Term}'", term);
                return null;
            }
        }

        public async Task MarkSetIsGenerated(int setId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId);
            if (set != null)
            {
                set.IsGenerated = true;
                await _unitOfWork.SaveChangesAsync();
            }
        }
        
        public async Task<List<FlashcardContextDTO>?> GenerateContextsForFlashcardAsync(int cardId)
        {
            var url = "interactions";

            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(cardId);
            if (flashcard == null) throw new Exception("Flashcard not found");

            var set = await _unitOfWork.Sets.GetByIdAsync(flashcard.SetId);
            if (flashcard == null) throw new Exception("Set not found");

            string prompt = $"Provide 3 natural example sentences in the original language for the term '{flashcard.Term}' " +
                            $"with their translations in {set?.ToLang} language. " +
                            $"In the 'sentence' field, wrap the exact word or phrase that matches '{flashcard.Term}' in <strong></strong> tags. " +
                            $"In the 'translation' field, wrap the word or phrase that semantically corresponds to '{flashcard.Term}' in <strong></strong> tags, " +
                            "even if it's a different grammatical form (e.g., a conjugated verb or declined noun). " +
                            "Do not use any other HTML tags. " +
                            "Return ONLY a JSON array of objects: [{'sentence': '...', 'translation': '...'}].";

            var requestBody = new
            {
                model = "gemini-2.5-flash",
                input = prompt
            };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(requestBody)
                };
                request.Headers.Add("x-goog-api-key", _apiKey);

                var response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini API returned status code: {StatusCode} during simple hint generation for '{Term}'", response.StatusCode, flashcard.Term);
                    return null;
                }

                var rawTextFromGoogle = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Raw response from Gemini API: " + rawTextFromGoogle);

                var result = JsonSerializer.Deserialize<GeminiResponse>(rawTextFromGoogle, _jsonOptions);

                var outputStep = result?.Steps?.FirstOrDefault(s => s.Type == "model_output");
                var rawJson = outputStep?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;

                if (string.IsNullOrEmpty(rawJson)) return new List<FlashcardContextDTO>();

                if (rawJson.Contains("```"))
                {
                    rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
                }

                var contextDtos = JsonSerializer.Deserialize<List<FlashcardContextDTO>>(rawJson, _jsonOptions);

                if (contextDtos != null)
                {
                    var sanitizer = new HtmlSanitizer(); //to prevent XSS attacks, allow only <strong> tags and no other attributes
                    sanitizer.AllowedTags.Clear();
                    sanitizer.AllowedTags.Add("strong");
                    sanitizer.AllowedAttributes.Clear(); // no other attributes allowed

                    foreach (var dto in contextDtos)
                    {
                        dto.Sentence = sanitizer.Sanitize(dto.Sentence);
                        dto.Translation = sanitizer.Sanitize(dto.Translation);
                    }
                }

                return contextDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating Gemini hint for term: '{Term}'", flashcard.Term);
                return null;
            }
        }
    }

    public class GeminiResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("steps")]
        public List<GeminiStep>? Steps { get; set; }
    }

    public class GeminiStep
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("content")]
        public List<GeminiContent>? Content { get; set; }
    }

    public class GeminiContent
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}