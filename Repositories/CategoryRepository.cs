using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Context;
using Core.Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(DataContext context) : base(context) { }
        public async Task UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _dbSet.FindAsync(category.Id);
            if (existingCategory is not null)
            {
                existingCategory.Name = category.Name;
            }
        }
    }
}
