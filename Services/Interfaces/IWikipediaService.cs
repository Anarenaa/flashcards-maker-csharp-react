namespace Services.Interfaces
{
    public interface IWikipediaService
    {
        Task<string?> GetDescriptionAsync(string term, string lang);
    }
}
