namespace Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        ISetRepository Sets { get; }
        IFlashcardRepository Flashcards { get; }
        Task SaveChangesAsync();
    }
}
