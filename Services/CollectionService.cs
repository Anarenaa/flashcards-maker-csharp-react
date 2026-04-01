using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Repositories.Interfaces;

namespace Services
{
    public class CollectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CollectionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<CollectionDTO>> GetCollectionsByUserIdAsync(int userId)
        {
            var collections = await _unitOfWork.Collections.GetAllAsync(
                filter: c => c.UserId == userId
             );

            return collections.Select(
                c => new CollectionDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    LastUpdatedAt = c.UpdatedAt
                }
            );
        }
        public async Task<CollectionDetailDTO> GetCollectionByIdAsync(int collectionId)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(
                collectionId,
                includeProperties: "Sets,User");
            if (collection == null)
                throw new NotFoundException("Колекція не знайдена");

            var setIds = collection.Sets.Select(s => s.Id).ToList();
            var flashcardCounts = await _unitOfWork.Flashcards.GetCountsBySetIdsAsync(setIds);

            return new CollectionDetailDTO
            {
                Id = collection.Id,
                Name = collection.Name,
                Description = collection.Description,
                Sets = collection.Sets.Select(s => new SetDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    UserName = s.User?.UserName ?? null,
                    IsPublic = s.IsPublic,
                    FlashcardsCount = flashcardCounts.GetValueOrDefault(s.Id, 0),
                    CreatedAt = s.CreatedAt,
                    LastUpdatedAt = s.UpdatedAt
                }).ToList()
            };
        }
        public async Task CreateCollectionAsync(CollectionDTO collectionDto, int userId)
        {
            var collection = new Collection
            {
                Name = collectionDto.Name,
                Description = collectionDto.Description,
                UserId = userId
            };
            await _unitOfWork.Collections.AddAsync(collection);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateCollectionAsync(CollectionDTO collectionDto)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(collectionDto.Id.Value);
            if (collection == null)
                throw new NotFoundException("Колекція не знайдена");

            collection.Name = collectionDto.Name;
            collection.Description = collectionDto.Description;
            collection.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteCollectionAsync(int collectionId)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(collectionId);
            if (collection == null)
                throw new NotFoundException("Колекція не знайдена");

            _unitOfWork.Collections.Delete(collection);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
