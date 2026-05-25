using Core.Context;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class SetRepository : Repository<Set>, ISetRepository
    {
        public SetRepository(DataContext context) : base(context) { }
        public async Task<int> GetUserSetsCount(int userId)
        {
            return await _dbSet.CountAsync(s => s.UserId == userId);
        }
        public async Task DeleteUserSets(int userId)
        {
            var userSets = await _dbSet.Where(s => s.UserId == userId).ToListAsync();
            _dbSet.RemoveRange(userSets);
        }
    }
}
