using Core.Context;
using Repositories.Interfaces;

namespace Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BaseDataContext _context;
        public ISetRepository Sets { get; }
        public IFlashcardRepository Flashcards { get; }
        public IFlashcardContextRepository FlashcardContexts { get; }
        public ICategoryRepository Categories { get; }
        public ICollectionRepository Collections { get; }
        public IReportRepository Reports { get; }
        public IPracticeRepository Practice { get; }
        public UnitOfWork(BaseDataContext context)
        {
            _context = context;
            Sets = new SetRepository(_context);
            Flashcards = new FlashcardRepository(_context);
            FlashcardContexts = new FlashcardContextsRepository(_context);
            Categories = new CategoryRepository(_context);
            Collections = new CollectionRepository(_context);
            Practice = new PracticeRepository(_context);
            Reports = new ReportRepository(_context);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
