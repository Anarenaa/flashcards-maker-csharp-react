namespace Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        ISetRepository Sets { get; }
        IFlashcardRepository Flashcards { get; }
        ICategoryRepository Categories { get; }
        ICollectionRepository Collections { get; }
        IPracticeRepository Practice { get;  }
        Task SaveChangesAsync();
    }
}
