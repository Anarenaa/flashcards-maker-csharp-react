using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace App.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/sets-ai")]
    public class SetsAIController : ControllerBase
    {
        private const string HARDCODED_API_KEY = "";

        [HttpGet("generate")]
        public async Task<IActionResult> Generate([FromQuery] string prompt, [FromQuery] int count)
        {
            if (string.IsNullOrEmpty(HARDCODED_API_KEY))
            {
                await Task.Delay(1500); 
                var mockCards = new List<FlashcardDTO>();
                for (int i = 1; i <= count; i++)
                {
                    mockCards.Add(new FlashcardDTO
                    {
                        Term = $"{prompt} #{i}",
                        Definition = $"Згенероване визначення для {prompt} номер {i}"
                    });
                }
                return Ok(mockCards);
            }

            try
            {
                using var client = new HttpClient();
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={HARDCODED_API_KEY}";

                var requestBody = new
                {
                    contents = new[] {
                        new { parts = new[] { new { text = $"Generate {count} cards for topic: {prompt}. Format: JSON array [{{'Term':'','Definition':''}}]. Language: Ukrainian." } } }
                    },
                    generationConfig = new { response_mime_type = "application/json" }
                };

                var response = await client.PostAsJsonAsync(url, requestBody);
                if (!response.IsSuccessStatusCode) return BadRequest("Google відхилив ключ");

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var rawJson = result.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                return Ok(rawJson);
            }
            catch { return Ok(new { error = "AI Offline" }); }
        }
    }
}