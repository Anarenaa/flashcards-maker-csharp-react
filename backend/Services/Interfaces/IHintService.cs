using Core.Models;

namespace Services.Interfaces
{
    public interface IHintService
    {
        Task<string> GetHintAsync(SetType type, string term, string fromLang, string toLang);
    }
}
