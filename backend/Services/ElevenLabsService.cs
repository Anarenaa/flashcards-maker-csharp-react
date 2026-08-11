using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Services.Interfaces;

public class ElevenLabsService : IElevenLabsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ElevenLabsService> _logger;
    private readonly IMemoryCache _cache;

    public ElevenLabsService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ElevenLabsService> logger,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ElevenLabs:ApiKey"]!;
        _logger = logger;
        _cache = cache;
    }

    public async Task<byte[]> SynthesizeSpeechAsync(string text, string? langCode)
    {
        string cacheKey = $"tts_{text}_{langCode ?? "default"}";

        if (_cache.TryGetValue(cacheKey, out byte[]? cachedAudio))
        {
            return cachedAudio!;
        }

        string voiceId = "JBFqnCBsd6RMkjVDRZzb"; //default
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";
        var requestBody = new
        {
            text = text,
            model_id = "eleven_v3",
            language_code = langCode
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody)
        };

        request.Headers.Add("xi-api-key", _apiKey);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error synthesizing speech. Status Code: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
        }
        response.EnsureSuccessStatusCode();

        byte[] audioBytes = await response.Content.ReadAsByteArrayAsync();

        _cache.Set(cacheKey, audioBytes, TimeSpan.FromDays(7));

        // mp3
        return audioBytes;
    }
}