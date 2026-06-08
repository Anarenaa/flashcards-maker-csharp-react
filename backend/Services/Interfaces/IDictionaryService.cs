namespace Services.Interfaces
{
    public interface IDictionaryService
    {
        Task<string?> GetTranslationAsync(string term, string fromLang, string toLang);
    }
}
