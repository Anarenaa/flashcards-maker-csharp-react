namespace Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        ISetRepository Sets { get; }
        IFlashcardRepository Flashcards { get; }
        ICategoryRepository Categories { get; }
        ICollectionRepository Collections { get; }
        Task SaveChangesAsync();
    }
}
