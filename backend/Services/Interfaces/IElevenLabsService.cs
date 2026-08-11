namespace Services.Interfaces
{
    public interface IElevenLabsService
    {
        Task<byte[]> SynthesizeSpeechAsync(string text, string? langCode);
    }
}
