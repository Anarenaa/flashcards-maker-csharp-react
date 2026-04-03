using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet.Actions;
using System.Text.RegularExpressions;

namespace Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public UserService(UserManager<User> userManager, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
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
        private async Task<string> uploadToCloudinaryAsync(IFormFile file, string fileName)
        {
            try
            {
                var cloudName = _configuration["Cloudinary:CloudName"];
                var apiKey = _configuration["Cloudinary:ApiKey"];
                var apiSecret = _configuration["Cloudinary:ApiSecret"];

                if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                {
                    throw new Exception("Cloudinary не налаштовано. Перевірте appsettings.json");
                }

                var account = new Account(cloudName, apiKey, apiSecret);
                var cloudinary = new Cloudinary(account);

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = "avatars",
                    Transformation = new Transformation().Width(150).Height(150).Crop("fill").Gravity("face"),
                    UseFilename = true,
                    UniqueFilename = false
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary помилка: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl?.ToString() ?? throw new Exception("Не вдалося отримати URL з Cloudinary");
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка завантаження в Cloudinary: {ex.Message}");
            }
        }
        public async Task UpdateUserProfileAsync(int id, string? userName = null, string? avatarUrl = null, IFormFile? avatarFile = null)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");

            bool hasChanges = false;

            if (!string.IsNullOrEmpty(userName) && userName != user.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(userName);
                if (existingUser != null && existingUser.Id != id)
                {
                    throw new Exception("Користувач з таким іменем вже існує");
                }
                user.UserName = userName;
                hasChanges = true;
            }

            if (avatarFile != null)
            {
                long maxFileSize = 5 * 1024 * 1024; // 5 MB
                if (avatarFile.Length > maxFileSize)
                {
                    throw new Exception("Файл занадто великий. Максимальний розмір — 5 МБ");
                }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(avatarFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("Недопустимий формат файлу. Дозволені: .jpg, .png, .webp");
                }

                // Видалення старого аватара якщо існує
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    await DeleteOldAvatarAsync(user.AvatarUrl);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var cloudinaryUrl = await uploadToCloudinaryAsync(avatarFile, fileName);
                user.AvatarUrl = cloudinaryUrl;
                hasChanges = true;
            }
            else if (!string.IsNullOrEmpty(avatarUrl) && avatarUrl != user.AvatarUrl)
            {
                // Видалення старого аватара якщо існує
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    await DeleteOldAvatarAsync(user.AvatarUrl);
                }
                
                user.AvatarUrl = avatarUrl;
                hasChanges = true;
            }

            if (hasChanges)
            {
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    throw new Exception("Не вдалося оновити профіль: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private async Task DeleteOldAvatarAsync(string oldAvatarUrl)
        {
            try
            {
                // Перевіряємо чи це Cloudinary URL
                if (!oldAvatarUrl.Contains("cloudinary.com"))
                {
                    return; // Це не Cloudinary файл, не видаляємо
                }

                // Витягуємо public ID з Cloudinary URL
                var publicId = ExtractPublicIdFromUrl(oldAvatarUrl);
                if (string.IsNullOrEmpty(publicId))
                {
                    return; // Не вдалося витягнути ID, пропускаємо
                }

                // Налаштування Cloudinary
                var cloudName = _configuration["Cloudinary:CloudName"];
                var apiKey = _configuration["Cloudinary:ApiKey"];
                var apiSecret = _configuration["Cloudinary:ApiSecret"];

                if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                {
                    return; // Cloudinary не налаштовано, не видаляємо
                }

                var account = new Account(cloudName, apiKey, apiSecret);
                var cloudinary = new Cloudinary(account);

                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.Result != "ok")
                {
                    Console.WriteLine($"Не вдалося видалити старий аватар: {deletionResult.Result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка видалення старого аватара: {ex.Message}");
            }
        }

        private string ExtractPublicIdFromUrl(string cloudinaryUrl)
        {
            try
            {
                // Стандартний формат з версією
                var pattern1 = @"/upload/v\d+/(.+?)(\.[a-zA-Z]{3,4})$";
                var match1 = Regex.Match(cloudinaryUrl, pattern1);
                
                if (match1.Success)
                {
                    return match1.Groups[1].Value; // Повертає "avatars/filename"
                }
                
                // Формат без версії
                var pattern2 = @"/upload/(.+?)(\.[a-zA-Z]{3,4})$";
                var match2 = Regex.Match(cloudinaryUrl, pattern2);
                
                if (match2.Success)
                {
                    return match2.Groups[1].Value; // Повертає "avatars/filename"
                }
                
                // З трансформаціями та версією
                var pattern3 = @"/upload/.+?/v\d+/(.+?)(\.[a-zA-Z]{3,4})$";
                var match3 = Regex.Match(cloudinaryUrl, pattern3);
                
                if (match3.Success)
                {
                    return match3.Groups[1].Value; // Повертає "avatars/filename"
                }
                
                // З трансформаціями без версії
                var pattern4 = @"/upload/.+?/(.+?)(\.[a-zA-Z]{3,4})$";
                var match4 = Regex.Match(cloudinaryUrl, pattern4);
                
                if (match4.Success)
                {
                    return match4.Groups[1].Value; // Повертає "avatars/filename"
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");

            // Видалення аватара користувача
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                await DeleteOldAvatarAsync(user.AvatarUrl);
            }

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
