using System.Linq.Expressions;

namespace Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        public Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string includeProperties = "");
        Task<T?> GetByIdAsync(int id, string includeProperties = "");
        Task AddAsync(T entity);

        //Справжня робота (сам SQL запит UPDATE або DELETE) відбувається пізніше —
        //у методі SaveChangesAsync(), який вже є асинхронним.
        void Delete(T entity);
    }
}
