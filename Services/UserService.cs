using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Core.Models.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public UserService(UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager, 
            IEmailService emailService,
            IUnitOfWork unitOfWork, 
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        public async Task<List<PublicUserDTO>> GetAllUsersAsync(int? currentUserId, string? roleFilter = null, string? searchTerm = null)
        {
            var query = _userManager.Users.Where(u => u.Id != currentUserId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
                var userIdsInRole = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();

                query = query.Where(u => u.Id.ToString().Contains(searchTerm) ||
                                         u.UserName.ToLower().Contains(searchTerm) ||
                                         u.Email.ToLower().Contains(searchTerm));
            }

            var users = await query.ToListAsync();
            var userDtos = new List<PublicUserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new PublicUserDTO
                {
                    Id = user.Id,
                    AvatarUrl = user.AvatarUrl,
                    UserName = user.UserName,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            return userDtos;
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

            await _unitOfWork.Sets.DeleteUserSets(user.Id);
            await _unitOfWork.Reports.DeleteUserReports(user.Id);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Не вдалося видалити профіль: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteUserByAdminAsync(int id, string reason)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");
            await _emailService.SendEmailAsync(
                user.Email,
                "Акаунт видалено",
                $"Ваш акаунт видалено адміном. Причина: {reason}"
            );
            await DeleteUserAsync(id);
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
                IsPublic = user.IsPublic,//аналогічно
                SetsCount = await _unitOfWork.Sets.GetUserSetsCount(userId),
                FlashcardsCount = await _unitOfWork.Flashcards.GetUserFlashcardsCount(userId),
                CollectionsCount = await _unitOfWork.Collections.GetUserCollectionsCount(userId),//бо не показує кількість колекцій
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
        public async Task SwitchProfilePublicity(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");
            user.IsPublic = !user.IsPublic;
            await _userManager.UpdateAsync(user);
        }
        public async Task<bool> IsProfilePrivate(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");
            return !user.IsPublic;
        }
        //roles management
        public async Task<bool> IsUserInRoleAsync(int userId, Roles role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, GetRoleName(role));
        }

        // Helper for converting enum to string
        private string GetRoleName(Roles role)
        {
            return role switch
            {
                Roles.User => RoleNames.User,
                Roles.Admin => RoleNames.Admin,
                _ => RoleNames.User
            };
        }

        public async Task<IdentityResult> CreateAdminAsync(string email, string password)
        {
            var adminRole = await _roleManager.RoleExistsAsync(RoleNames.Admin);
            if (!adminRole)
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(RoleNames.Admin));
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleNames.Admin);
            }

            return result;
        }
        public async Task ToggleUserBlockAsync(int userId, bool shouldBlock, int? days = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return;

            if (shouldBlock)
            {
                DateTimeOffset lockoutEnd = days.HasValue && days.Value > 0
                    ? DateTimeOffset.UtcNow.AddDays(days.Value)
                    : DateTimeOffset.MaxValue;

                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

                user.IsBanned = !days.HasValue;
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                user.IsBanned = false;
            }

            await _userManager.UpdateSecurityStampAsync(user);

            await _userManager.UpdateAsync(user);
        }
        public async Task<List<int>> GetBlockedUserIdsAsync()
        {
            var now = DateTimeOffset.UtcNow;

            return await _userManager.Users
                .Where(u => u.IsBanned || (u.LockoutEnd != null && u.LockoutEnd > now))
                .Select(u => u.Id)
                .ToListAsync();
        }
    }
}
