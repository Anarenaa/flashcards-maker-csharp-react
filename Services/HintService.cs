using Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    public class HintService : IHintService
    {
        private readonly IWikipediaService _wikiService;
        private readonly IDictionaryService _dictionaryService;
        private readonly IGeminiService _geminiService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HintService> _logger;

        public HintService(
            IWikipediaService wikiService,
            IDictionaryService dictionaryService,
            IGeminiService geminiService,
            IMemoryCache cache,
            ILogger<HintService> logger)
        {
            _wikiService = wikiService;
            _dictionaryService = dictionaryService;
            _geminiService = geminiService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string> GetHintAsync(string term, SetType type, string? fromLang, string? toLang, string? uiLang)
        {
            string cacheKey = $"hint_{type}_{fromLang ?? ""}_{toLang ?? ""}_{uiLang ?? ""}_{term.ToLower().Trim()}";

            if (_cache.TryGetValue(cacheKey, out string? cachedHint))
            {
                _logger.LogInformation("Hint for '{Term}' found in cache.", term);
                return cachedHint!;
            }

            _logger.LogInformation("Hint for '{Term}' not found in cache. Fetching from external APIs...", term);
            string? result = null;

            if (type == SetType.Language)
            {
                result = await _dictionaryService.GetTranslationAsync(term, fromLang, toLang);
            }
            else if (type == SetType.Subject)
            {
                result = await _wikiService.GetDescriptionAsync(term, uiLang);
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                _logger.LogWarning("External API failed or returned empty result for '{Term}'. Falling back to Gemini...", term);

                string targetLang = type == SetType.Language ? toLang : uiLang;
                result = await _geminiService.GenerateSimpleHintAsync(term, targetLang, type);
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = "Підказка недоступна, спробуйте пізніше";
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));

            _cache.Set(cacheKey, result, cacheOptions);

            return result;
        }
    }
}