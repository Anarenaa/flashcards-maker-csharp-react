namespace Services.Interfaces
{
    public interface IWikipediaService
    {
        Task<string?> GetDefinitionAsync(string term, string lang);
    }
}
