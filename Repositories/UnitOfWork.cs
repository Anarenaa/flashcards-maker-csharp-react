using Core.Context;
using Repositories.Interfaces;

namespace Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        public ISetRepository Sets { get; }
        public IFlashcardRepository Flashcards { get; }
        public ICategoryRepository Categories { get; }
        public ICollectionRepository Collections { get; }
        //public IUserRepository Users { get; }
        public UnitOfWork(DataContext context)
        {
            _context = context;
            Sets = new SetRepository(_context);
            Flashcards = new FlashcardRepository(_context);
            Categories = new CategoryRepository(_context);
            Collections = new CollectionRepository(_context);
            //Users = new UserRepository(_context);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
