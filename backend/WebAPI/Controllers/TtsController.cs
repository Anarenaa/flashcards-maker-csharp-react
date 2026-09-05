using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

[Route("api/tts")]
[ApiController]
public class TtsController : ControllerBase
{
    private readonly IElevenLabsService _ttsService;

    public TtsController(IElevenLabsService ttsService)
    {
        _ttsService = ttsService;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateSpeech([FromBody] TtsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Text))
            return BadRequest(new { error = "Text is required." });

        byte[] audioBytes = await _ttsService.SynthesizeSpeechAsync(request.Text, request.Language);

        // mp3
        return File(audioBytes, "audio/mpeg");
    }
}

public class TtsRequest
{
    public required string Text { get; set; }
    public string? Language { get; set; }
}