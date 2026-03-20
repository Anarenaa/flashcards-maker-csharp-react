namespace Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        ISetRepository Sets { get; }
        Task SaveChangesAsync();
    }
}
