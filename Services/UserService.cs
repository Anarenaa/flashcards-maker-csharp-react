using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Repositories.Interfaces;

namespace Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public UserService(UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }
        public async Task<PrivateUserDTO> GetMyPrivateProfileAsync(int myId)
        {
            var user = await _userManager.FindByIdAsync(myId.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");

            return new PrivateUserDTO
            {
                Id = user.Id,
                AvatarUrl = user.AvatarUrl,
                UserName = user.UserName,
                Email = user.Email,
                CollectionsCount = await _unitOfWork.Collections.GetUserCollectionsCount(user.Id),
                SetsCount = await _unitOfWork.Sets.GetUserSetsCount(user.Id),
                FlashcardsCount = await _unitOfWork.Flashcards.GetUserFlashcardsCount(user.Id),
                CreatedAt = user.CreatedAt
            };
        }
        public async Task UpdateMyProfileAvatarAsync(int id, string path)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");

            user.AvatarUrl = path;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Не вдалося оновити аватар: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        public async Task DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");

            await _unitOfWork.Sets.DeleteUnusedUserSetsAsync(user.Id);
            await _unitOfWork.Sets.UnableSetsWithoutUserInCollections();
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Не вдалося видалити профіль: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<PublicUserDTO> GetUserProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");

            var sets = await _unitOfWork.Sets.GetAllAsync(s => s.IsPublic && s.UserId == user.Id);
            var setIds = sets.Select(s => s.Id).ToList();

            var flashcardsCounts = await _unitOfWork.Flashcards.GetCountsBySetIdsAsync(setIds);

            return new PublicUserDTO
            {
                Id = user.Id,
                AvatarUrl = user.AvatarUrl,
                UserName = user.UserName,
                Email = user.Email,
                SetsCount = await _unitOfWork.Sets.GetUserSetsCount(userId),
                FlashcardsCount = await _unitOfWork.Flashcards.GetUserFlashcardsCount(userId),
                CreatedAt = user.CreatedAt,
                Sets = sets.Select(s => new SetDTO
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        FlashcardsCount = flashcardsCounts.GetValueOrDefault(s.Id, 0),
                        IsPublic = s.IsPublic,
                        CreatedAt = s.CreatedAt,
                        LastUpdatedAt = s.UpdatedAt
                }).ToList()
            };
        }
    }
}
