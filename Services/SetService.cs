using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Repositories.Interfaces;

namespace Services
{
    public class SetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public SetService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<List<SetDTO>> GetAllSetsAsync(int currentUserId, List<int>? categoryIds, string? searchText = null)
        {
            var sets = await _unitOfWork.Sets.GetAllAsync(
                    filter: s => s.IsPublic
                    && s.UserId != currentUserId
                    && (categoryIds == null || !categoryIds.Any() || s.Categories.Any(c => categoryIds.Contains(c.Id)))
                    && (string.IsNullOrEmpty(searchText) || s.Name.Contains(searchText)),
                    includeProperties: "User"
            );

            var setIds = sets.Select(s => s.Id).ToList();
            var flashcardCounts = await _unitOfWork.Flashcards.GetCountsBySetIdsAsync(setIds);

            var setDtos = sets.Select(s => new SetDTO
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                FlashcardsCount = flashcardCounts.GetValueOrDefault(s.Id, 0),
                UserName = s.User?.UserName ?? null,
                IsPublic = s.IsPublic,
                CreatedAt = s.CreatedAt,
                LastUpdatedAt = s.UpdatedAt
            }).ToList();

            return setDtos;
        }
        public async Task<List<SetDTO>> GetAllUserSetsAsync(int userId, List<int>? categoryIds, string? searchText = null)
        {
            var sets = await _unitOfWork.Sets.GetAllAsync(
                    filter: s => s.UserId == userId &&
                    (categoryIds == null || !categoryIds.Any() || s.Categories.Any(c => categoryIds.Contains(c.Id)))
                    && (string.IsNullOrEmpty(searchText) || s.Name.Contains(searchText))
            );

            var setIds = sets.Select(s => s.Id).ToList();
            var flashcardCounts = await _unitOfWork.Flashcards.GetCountsBySetIdsAsync(setIds);

            var setDtos = sets.Select(s => new SetDTO
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                FlashcardsCount = flashcardCounts.GetValueOrDefault(s.Id, 0),
                IsPublic = s.IsPublic,
                CreatedAt = s.CreatedAt,
                LastUpdatedAt = s.UpdatedAt
            }).ToList();

            return setDtos;
        }
        public async Task<List<SetDTO>> GetSetsByCollectionIdAsync(int collectionId)
        {
            var sets = await _unitOfWork.Sets.GetAllAsync(
                    filter: s => s.Collections.Any(c => c.Id == collectionId),
                    includeProperties: "User"
            );
            var setIds = sets.Select(s => s.Id).ToList();
            var flashcardCounts = await _unitOfWork.Flashcards.GetCountsBySetIdsAsync(setIds);
            var setDtos = sets.Select(s => new SetDTO
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                FlashcardsCount = flashcardCounts.GetValueOrDefault(s.Id, 0),
                UserName = s.User?.UserName ?? null,
                IsPublic = s.IsPublic,
                CreatedAt = s.CreatedAt,
                LastUpdatedAt = s.UpdatedAt
            }).ToList();
            return setDtos;
        }
        public async Task<SetDetailDTO> GetSetByIdAsync(int setId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId, "Flashcards,Categories");

            if (set == null) throw new NotFoundException("Сет не знайдено");

            return new SetDetailDTO
            {
                Id = set.Id,
                Name = set.Name,
                Description = set.Description,
                Type = set.Type,
                FlashcardsCount = set.Flashcards.Count(),
                IsPublic = set.IsPublic,
                CreatedAt = set.CreatedAt,
                LastUpdatedAt = set.UpdatedAt,
                Flashcards = set.Flashcards
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FlashcardDTO
                {
                    Id = f.Id,
                    Term = f.Term,
                    Definition = f.Definition
                }).ToList(),
                Categories = set.Categories
                .OrderByDescending(f => f.CreatedAt)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList()
            };
        }

        public async Task AddSetAsync(SetDTO setDto, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var set = new Set
            {
                Name = setDto.Name,
                Description = setDto.Description,
                Type = setDto.Type,
                IsPublic = setDto.IsPublic,
                IsGenerated = setDto.IsGenerated,
                UserId = userId,
                User = user
            };
            await _unitOfWork.Sets.AddAsync(set);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<Set> AddSetAsyncWithReturn(SetCreateDTO setDto, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var set = new Set
            {
                Name = setDto.Name,
                Description = setDto.Description,
                Type = setDto.Type,
                IsPublic = setDto.IsPublic,
                UserId = userId,
                User = user
            };
            await _unitOfWork.Sets.AddAsync(set);
            await _unitOfWork.SaveChangesAsync();
            return set;
        }
        public async Task UpdateSetAsync(int setId, SetCreateDTO setDto)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId);
            if (set is null)
            {
                throw new NotFoundException("Сет не знайдено");
            }

            set.Name = setDto.Name;
            set.Description = setDto.Description;
            set.Type = setDto.Type;
            set.IsPublic = setDto.IsPublic;
            set.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteSetAsync(int id)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(id);
            if (set is not null)
            {
                _unitOfWork.Sets.Delete(set);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new NotFoundException("Сет не знайдено");
            }
        }

        public async Task AddCategoryToSetAsync(int setId, int categoryId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(
                setId,
                includeProperties: "Categories"
            );
            if (set == null) throw new NotFoundException("Сет не знайдено");

            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null) throw new NotFoundException("Категорію не знайдено");

            if (!set.Categories.Any(c => c.Id == categoryId))
            {
                set.Categories.Add(category);
            }
            else
            {
                throw new AppException("Категорія вже додана до сету");
            }

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task RemoveCategoryFromSetAsync(int setId, int categoryId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(
                setId,
                includeProperties: "Categories"
            );
            if (set == null) throw new NotFoundException("Сет не знайдено");

            var category = set.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null) throw new NotFoundException("Категорію не знайдено в сеті");

            set.Categories.Remove(category);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task AddSetToCollectionAsync(int setId, int collectionId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId);
            if (set == null) throw new NotFoundException("Сет не знайдено");

            bool alreadyExists = await _unitOfWork.Collections
                .AnySetInCollectionAsync(collectionId, setId);

            if (alreadyExists) throw new AppException("Сет вже доданий до колекції");

            var collection = await _unitOfWork.Collections.GetByIdAsync(collectionId);
            if (collection == null) throw new NotFoundException("Колекцію не знайдено");

            collection.Sets.Add(set);
            collection.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task RemoveSetFromCollectionAsync(int setId, int collectionId)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(collectionId);
            if (collection == null) throw new NotFoundException("Колекцію не знайдено");

            var set = await _unitOfWork.Collections.LoadSingleSetAsync(collection, setId);
            if (set == null) throw new NotFoundException("Сет не знайдено в колекції");

            collection.Sets.Remove(set);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CopySetToUser(int setId, int userId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId, "Flashcards, Categories");
            if (set == null) throw new NotFoundException("Сет не знайдено");
            var newSet = new Set
            {
                Name = set.Name + " (Копія)",
                Description = set.Description,
                IsPublic = false,
                UserId = userId,
                Categories = set.Categories.ToList()
            };
            foreach (var flashcard in set.Flashcards)
            {
                var newFlashcard = new Flashcard
                {
                    Term = flashcard.Term,
                    Definition = flashcard.Definition
                };
                newSet.Flashcards.Add(newFlashcard);
            }
            await _unitOfWork.Sets.AddAsync(newSet);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> IsSetMine(int setId, int userId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId);
            if (set == null) throw new NotFoundException("Сет не знайдено");
            return set.UserId == userId;
        }
    }
}
