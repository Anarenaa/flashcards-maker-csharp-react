using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Repositories;
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
        private async Task<List<SetDTO>> mapToSetDtosAsync(IEnumerable<Set> items, int userId)
        {
            var setIds = items.Select(s => s.Id).ToList();
            if (!setIds.Any()) return new List<SetDTO>();

            var flashcardCounts = await _unitOfWork.Flashcards.GetCountsBySetIdsAsync(setIds);
            var progressMap = await _unitOfWork.Sets.GetOverallProgressForSetsAsync(userId, setIds);

            return items.Select(s => new SetDTO
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                FromLang = s.FromLang,
                ToLang = s.ToLang,
                FlashcardsCount = flashcardCounts.GetValueOrDefault(s.Id, 0),
                AvatarUrl = s.User?.AvatarUrl,
                UserName = s.User?.UserName,
                IsPublic = s.IsPublic,
                IsGenerated = s.IsGenerated,
                CreatedAt = s.CreatedAt,
                LastUpdatedAt = s.UpdatedAt,
                OverallProgress = progressMap.GetValueOrDefault(s.Id, 0f)
            }).ToList();
        }
        public async Task<PagedResult<SetDTO>> GetAllSetsAsync(
            int page,
            int perPage,
            int currentUserId,
            int? categoryId,
            SetType? setType,
            string? fromLangCode,
            string? searchText = null,
            string? progress = null)
        {
            List<int>? filteredSetIds = null;

            if (!string.IsNullOrEmpty(progress))
            {
                filteredSetIds = await _unitOfWork.Sets.FilterSetIdsByProgressAsync(currentUserId, progress, isMySets: false);
            }
            var setsPagedResult = await _unitOfWork.Sets.GetAllPagedAsync(
                    page: page,
                    perPage: perPage,
                    filter: s => s.IsPublic
                    && s.UserId != currentUserId
                    && s.Flashcards.Count > 0
                    && (filteredSetIds == null || filteredSetIds.Contains(s.Id))
                    && (categoryId == null || s.Categories.Any(c => c.Id == categoryId))
                    && (setType == null || s.Type == setType)
                    && (string.IsNullOrEmpty(fromLangCode) || s.FromLang == fromLangCode)
                    && (string.IsNullOrEmpty(searchText) || s.Name.Contains(searchText)),
                    includeProperties: "User"
            );

            var dtos = await mapToSetDtosAsync(setsPagedResult.Items, currentUserId);
            return new PagedResult<SetDTO>(dtos, setsPagedResult.TotalItems, page, perPage);
        }
        public async Task<PagedResult<SetDTO>> GetAllUserSetsAsync(
            int page,
            int perPage,
            int currentUserId,
            int? categoryId,
            SetType? setType,
            string? fromLangCode,
            string? searchText = null,
            string? progress = null)
        {
            List<int>? filteredSetIds = null;

            if (!string.IsNullOrEmpty(progress))
            {
                filteredSetIds = await _unitOfWork.Sets.FilterSetIdsByProgressAsync(currentUserId, progress, isMySets: true);
            }
            var setsPagedResult = await _unitOfWork.Sets.GetAllPagedAsync(
                    page: page,
                    perPage: perPage,
                    filter: s => s.IsPublic
                    && s.UserId == currentUserId
                    && s.Flashcards.Count > 0
                    && (filteredSetIds == null || filteredSetIds.Contains(s.Id))
                    && (categoryId == null || s.Categories.Any(c => c.Id == categoryId))
                    && (setType == null || s.Type == setType)
                    && (string.IsNullOrEmpty(fromLangCode) || s.FromLang == fromLangCode)
                    && (string.IsNullOrEmpty(searchText) || s.Name.Contains(searchText)),
                    includeProperties: "User"
            );

            var dtos = await mapToSetDtosAsync(setsPagedResult.Items, currentUserId);
            return new PagedResult<SetDTO>(dtos, setsPagedResult.TotalItems, page, perPage);
        }
        public async Task<List<SetDTO>> GetSetsByCollectionIdAsync(int collectionId, int userId)
        {
            var sets = await _unitOfWork.Sets.GetAllAsync(
                    filter: s => s.Collections.Any(c => c.Id == collectionId),
                    includeProperties: "User"
            );

            return await mapToSetDtosAsync(sets, userId);
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
            collection.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> IsSetMine(int setId, int userId)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(setId);
            if (set == null) throw new NotFoundException("Сет не знайдено");
            return set.UserId == userId;
        }
        public IEnumerable<object> GetSetTypes()
        {
            return Enum.GetValues(typeof(SetType))
                .Cast<SetType>()
                .Select(t => new
                {
                    Id = (int)t,
                    Name = t.GetType()
                            .GetField(t.ToString())?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName() ?? t.ToString()
                });
        }
    }
}
