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
                AvatarUrl = s.User?.AvatarUrl ?? null,
                UserName = s.User?.UserName ?? null,
                IsPublic = s.IsPublic,
                IsGenerated = s.IsGenerated,
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
                FromLang = s.FromLang,
                ToLang = s.ToLang,
                FlashcardsCount = flashcardCounts.GetValueOrDefault(s.Id, 0),
                IsPublic = s.IsPublic,
                IsGenerated = s.IsGenerated,
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
                FromLang = s.FromLang,
                ToLang = s.ToLang,
                FlashcardsCount = flashcardCounts.GetValueOrDefault(s.Id, 0),
                AvatarUrl = s.User?.AvatarUrl ?? null,
                UserName = s.User?.UserName ?? null,
                IsPublic = s.IsPublic,
                IsGenerated = s.IsGenerated,
                CreatedAt = s.CreatedAt,
                LastUpdatedAt = s.UpdatedAt
            }).ToList();
            return setDtos;
        }
        public async Task<List<SetWithProgressDTO>> GetSetsWithProgress(int userId, List<int>? categoryIds, string? searchText = null)
        {
            var allUserProgress = await _unitOfWork.Practice.GetAllUserProgressAsync(userId);

            var activeProgress = allUserProgress.Where(p => p.Progress > 0f && p.Flashcard?.Set != null).ToList();

            if (!activeProgress.Any()) return new List<SetWithProgressDTO>();

            var setDtos = activeProgress
                .GroupBy(p => p.Flashcard.Set)
                .Where(g =>
                    (categoryIds == null || !categoryIds.Any() || g.Key.Categories.Any(c => categoryIds.Contains(c.Id))) &&
                    (string.IsNullOrEmpty(searchText) || g.Key.Name.Contains(searchText))
                )
                .Select(g => new SetWithProgressDTO
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Description = g.Key.Description,
                    Type = g.Key.Type,
                    FromLang = g.Key.FromLang,
                    ToLang = g.Key.ToLang,
                    FlashcardsCount = g.Key.Flashcards?.Count ?? 0,
                    IsPublic = g.Key.IsPublic,
                    IsGenerated = g.Key.IsGenerated,
                    CreatedAt = g.Key.CreatedAt,
                    LastUpdatedAt = g.Key.UpdatedAt,
                    OverallProgress = g.Average(p => p.Progress)
                })
                .OrderBy(x => x.OverallProgress)
                .ToList();

            return setDtos;
        }
        public async Task ResetSetProgressAsync(int userId, int setId)
        {
            var setProgress = await _unitOfWork.Practice.GetSetProgressAsync(userId, setId);

            if (setProgress.Any())
            {
                _unitOfWork.Practice.DeleteRange(setProgress);
                await _unitOfWork.SaveChangesAsync();
            }
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
                FromLang = set.FromLang,
                ToLang = set.ToLang,
                FlashcardsCount = set.Flashcards.Count(),
                IsPublic = set.IsPublic,
                IsGenerated = set.IsGenerated,
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

        public async Task<SetDTO> AddSetAsync(SetCreateDTO setDto, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var set = new Set
            {
                Name = setDto.Name,
                Description = setDto.Description,
                Type = setDto.Type,
                FromLang = string.IsNullOrEmpty(setDto.FromLang) ? null : setDto.FromLang,
                ToLang = string.IsNullOrEmpty(setDto.ToLang) ? null : setDto.ToLang,
                IsPublic = setDto.IsPublic,
                UserId = userId,
                User = user
            };
            await _unitOfWork.Sets.AddAsync(set);
            await _unitOfWork.SaveChangesAsync();

            return new SetDTO
            {
                Id = set.Id,
                Name = set.Name,
                Description = set.Description,
                Type = set.Type,
                FromLang = set.FromLang,
                ToLang = set.ToLang,
                AvatarUrl = user?.AvatarUrl ?? null,
                UserName = user?.UserName ?? null,
                IsPublic = set.IsPublic,
                CreatedAt = set.CreatedAt,
                LastUpdatedAt = set.UpdatedAt
            };
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
            set.FromLang = string.IsNullOrEmpty(setDto.FromLang) ? null : setDto.FromLang; ;
            set.ToLang = string.IsNullOrEmpty(setDto.ToLang) ? null : setDto.ToLang;
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

        public async Task<bool> IsSetMine(int setId, int userId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId);
            if (set == null) throw new NotFoundException("Сет не знайдено");
            return set.UserId == userId;
        }
    }
}
