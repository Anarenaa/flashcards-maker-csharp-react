using Core.Models;

namespace Services.Interfaces
{
    public interface IHintService
    {
        Task<string> GetHintAsync(string term, SetType type, string? fromLang, string? toLang, string? uiLang);
    }
}
