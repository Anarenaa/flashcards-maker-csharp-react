using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Repositories.Interfaces;

namespace Services
{
    public class CategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<CategoryDTO>> GetCategoriesByUserIdAsync(int userId)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(
                filter: c => c.UserId == userId
            );
            return categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name
            });
        }
        public async Task CreateCategoryAsync(CategoryDTO categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name
            };
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateCategoryAsync(CategoryDTO categoryDto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryDto.Id.Value);
            if (category == null)
            {
                throw new NotFoundException("Категорія не знайдена");
            }
            await _unitOfWork.Categories.UpdateCategoryAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException("Категорія не знайдена");
            }
            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
