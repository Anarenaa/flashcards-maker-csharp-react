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

        // --- UNIVERSAL PRIVATE METHOD FOR GEMINI API REQUESTS ---
        private async Task<GeminiResponse?> SendGeminiRequestAsync(object requestBody, string logContextName)
        {
            var url = "interactions";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("x-goog-api-key", _apiKey);

            var response = await _client.SendAsync(request);

            // Handle rate limit (Too Many Requests)
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException(
                    "Gemini API rate limit exceeded.",
                    null,
                    System.Net.HttpStatusCode.TooManyRequests
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini API returned status code: {StatusCode} during {Context}", response.StatusCode, logContextName);
                return null;
            }

            var rawTextFromGoogle = await response.Content.ReadAsStringAsync();

            // Deserialize response and log token usage metadata
            var result = JsonSerializer.Deserialize<GeminiResponse>(rawTextFromGoogle, _jsonOptions);

            if (result?.Usage != null)
            {
                _logger.LogInformation(
                    "Gemini Tokens Used ({Context}) -> Prompt: {Prompt}, Candidates: {Candidates}, Total: {Total}",
                    logContextName,
                    result.Usage.PromptTokens,
                    result.Usage.CandidatesTokens,
                    result.Usage.TotalTokens
                );
            }

            return result;
        }

        public async Task<(SetCreateDTO? SetDto, List<FlashcardDTO>? Flashcards)> GenerateSetWithFlashcardsAsync(SetAIPromtCreateDTO requestDto)
        {
            object inputData;
            string text = $"{requestDto.Prompt}. " +
                        $"Type \"{requestDto.Type}\". " +
                        (requestDto.CardsCount.HasValue ? $"Generate {requestDto.CardsCount} cards. " : "") +
                        (requestDto.Type.ToString() == "Language" ? $"From {requestDto.FromLang} to {requestDto.ToLang}. " : $"In {requestDto.ToLang}. ") +
                        "Format ONLY as a JSON object: " +
                        "{ \"Name\": \"...\", \"Description\": \"...\", \"Flashcards\": [{ \"Term\": \"...\", \"Definition\": \"...\" }] }. " +
                        "Make response clear and simple. Without transcription. Terms and definitions should be short enough to be able to write them.";

            byte[]? fileBytes = null;
            string? mimeType = null;

            if (requestDto.File != null && requestDto.File.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await requestDto.File.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
                mimeType = requestDto.File.ContentType;
            }

            if (fileBytes != null && !string.IsNullOrEmpty(mimeType))
            {
                string fileType = mimeType.StartsWith("image/") ? "image" : "file";
                inputData = new object[]
                {
                    new { type = "text", text },
                    new { type = fileType, data = Convert.ToBase64String(fileBytes), mime_type = mimeType }
                };
            }
            else
            {
                inputData = text;
            }

            var requestBody = new { model = "gemini-2.5-flash", input = inputData };

            var result = await SendGeminiRequestAsync(requestBody, $"Set Generation ('{requestDto.Prompt}')");
            if (result == null) return (null, new List<FlashcardDTO>());

            var outputStep = result.Steps?.FirstOrDefault(s => s.Type == "model_output");
            var rawJson = outputStep?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;

            if (string.IsNullOrEmpty(rawJson)) return (null, new List<FlashcardDTO>());

            if (rawJson.Contains("```"))
            {
                rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
            }

            var aiResult = JsonSerializer.Deserialize<GeneratedAiSetResponse>(rawJson, _jsonOptions);
            if (aiResult == null) return (null, new List<FlashcardDTO>());

            var setDto = new SetCreateDTO
            {
                Name = aiResult.Name,
                Description = aiResult.Description,
                Type = requestDto.Type,
                FromLang = requestDto.FromLang,
                ToLang = requestDto.ToLang,
                IsPublic = false
            };

            return (setDto, aiResult.Flashcards);
        }

        public async Task<string?> GenerateSimpleHintAsync(string term, string lang, SetType type)
        {
            string prompt = type == SetType.Language
                ? $"Translate the word '{term}' to language with ISO code '{lang}'. Return ONLY the translated word/phrase, no extra text, no explanations, no quotes."
                : $"Give a one-sentence definition of the term '{term}' in language with ISO code '{lang}'. Max 15 words. Return ONLY the definition text, no extra words.";

            var requestBody = new { model = "gemini-2.5-flash", input = prompt };

            try
            {
                var result = await SendGeminiRequestAsync(requestBody, $"Simple Hint ('{term}')");
                if (result == null) return null;

                var outputStep = result.Steps?.FirstOrDefault(s => s.Type == "model_output");
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

        public async Task<List<FlashcardContextDTO>?> GenerateContextsForFlashcardAsync(int cardId)
        {
            var flashcard = await _unitOfWork.Flashcards.GetByIdAsync(cardId);
            if (flashcard == null) throw new Exception("Flashcard not found");

            var set = await _unitOfWork.Sets.GetByIdAsync(flashcard.SetId);
            if (set == null) throw new Exception("Set not found");

            string prompt = $"Provide 3 short, natural example sentences in the original language using the EXACT term '{flashcard.Term}' " +
                (set.FromLang != set.ToLang ? $"with their translations in {set.ToLang} language. " : "") +
                $"Each sentence must be concise (maximum 10-12 words). " +
                $"In the 'sentence' field, wrap the exact term '{flashcard.Term}' in <strong></strong> tags (do NOT use synonyms or different forms of the word, use the exact term). " +
                (set.FromLang != set.ToLang
                ? $"In the 'translation' field, wrap the corresponding word or phrase in <strong></strong> tags."
                : "") +
                "Do NOT use any other HTML tags or Markdown asterisks. " +
                "Return ONLY a JSON array of objects: " +
                (set.FromLang != set.ToLang
                ? "[{'sentence': '...', 'translation': ''}]."
                : "[{'sentence': '...'}].");

            var requestBody = new { model = "gemini-2.5-flash", input = prompt };

            try
            {
                var result = await SendGeminiRequestAsync(requestBody, $"Contexts Generation ('{flashcard.Term}')");
                if (result == null) return new List<FlashcardContextDTO>();

                var outputStep = result.Steps?.FirstOrDefault(s => s.Type == "model_output");
                var rawJson = outputStep?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;

                if (string.IsNullOrEmpty(rawJson)) return new List<FlashcardContextDTO>();

                if (rawJson.Contains("```"))
                {
                    rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
                }

                var contextDtos = JsonSerializer.Deserialize<List<FlashcardContextDTO>>(rawJson, _jsonOptions);

                if (contextDtos != null)
                {
                    // Sanitize HTML output to prevent XSS attacks, allowing only <strong> tags
                    var sanitizer = new HtmlSanitizer();
                    sanitizer.AllowedTags.Clear();
                    sanitizer.AllowedTags.Add("strong");
                    sanitizer.AllowedAttributes.Clear();

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
                _logger.LogError(ex, "Error occurred while generating contexts for term: '{Term}'", flashcard.Term);
                return null;
            }
        }
    }

    // Response models and DTOs
    public class GeneratedAiSetResponse
    {
        [JsonPropertyName("Name")]
        public required string Name { get; set; }

        [JsonPropertyName("Description")]
        public required string Description { get; set; }

        [JsonPropertyName("Flashcards")]
        public required List<FlashcardDTO> Flashcards { get; set; }
    }

    public class GeminiUsage
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokens { get; set; }

        [JsonPropertyName("totalTokenCount")]
        public int TotalTokens { get; set; }
    }

    public class GeminiResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("steps")]
        public List<GeminiStep>? Steps { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsage? Usage { get; set; }
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