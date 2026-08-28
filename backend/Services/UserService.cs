using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.DTOs.Users;
using Core.Exceptions;
using Core.Models;
using Core.Models.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UserService> _logger;
        public UserService(UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager, 
            IEmailService emailService,
            IUnitOfWork unitOfWork, 
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<List<UserDTO>> GetAllUsersAsync(int? currentUserId, string? roleFilter = null, string? searchTerm = null)
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
                                         u.UserName!.ToLower().Contains(searchTerm) ||
                                         u.Email!.ToLower().Contains(searchTerm));
            }

            var users = await query.ToListAsync();
            var userDtos = new List<UserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDTO
                {
                    Id = user.Id,
                    AvatarUrl = user.AvatarUrl,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            return userDtos;
        }
        public async Task<IUserProfileDTO> GetUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new KeyNotFoundException("Користувача не знайдено");

            if (!user.IsPublic)
            {
                return new PrivateUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    AvatarUrl = user.AvatarUrl,
                    IsPublic = false 
                };
            }

            var allCardProgresses = await _unitOfWork.Practice.GetAllUserProgressAsync(id);

            int completedSets = 0;
            int masteredCards = 0;
            DateTime? lastActivity = null;

            if (allCardProgresses.Any())
            {
                var setGroups = allCardProgresses.GroupBy(cp => cp.Flashcard.SetId);
                foreach (var setGroup in setGroups)
                {
                    var setCardProgresses = setGroup.ToList();
                    var setOverallProgress = setCardProgresses.Average(cp => cp.Progress);

                    if (setOverallProgress >= 1.0f)
                    {
                        completedSets++;
                    }
                }

                masteredCards = allCardProgresses.Count(cp => cp.Progress >= 1.0f);
                lastActivity = allCardProgresses.Max(cp => cp.LastReview);
            }

            return new UserDTO
            {
                Id = user.Id,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                AvatarUrl = user.AvatarUrl,
                UserName = user.UserName!,
                Email = user.Email!,
                IsPublic = user.IsPublic,
                CreatedAt = user.CreatedAt,
                LastActivity = lastActivity,
                SetsCount = await _unitOfWork.Sets.GetUserSetsCount(user.Id),
                PublicSetsCount = await _unitOfWork.Sets.GetUserSetsCount(user.Id, filter: s => s.IsPublic == true),
                CollectionsCount = await _unitOfWork.Collections.GetUserCollectionsCount(user.Id),
                FlashcardsCount = await _unitOfWork.Flashcards.GetUserFlashcardsCount(user.Id),
                CompletedSets = completedSets,
                MasteredCards = masteredCards
            };
        }
        public async Task SwitchProfilePublicity(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");
            user.IsPublic = !user.IsPublic;
            await _userManager.UpdateAsync(user);
        }
        public async Task DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new NotFoundException("Користувача не знайдено");

            // Видалення аватара користувача
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                await deleteOldAvatarAsync(user.AvatarUrl);
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
                user.Email!,
                "Акаунт видалено",
                $"Ваш акаунт видалено адміном. Причина: {reason}"
            );
            await DeleteUserAsync(id);
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

        // update user profile with heplers
        public async Task UpdateUserProfileAsync(int id, string? userName = null, IFormFile? avatarFile = null, bool removeAvatar = false)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning("Attempt to update profile for non-existent user with ID: {UserId}", id);
                throw new KeyNotFoundException("Користувача не знайдено");
            }

            bool hasChanges = false;

            if (!string.IsNullOrEmpty(userName) && userName != user.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(userName);
                if (existingUser != null && existingUser.Id != id)
                {
                    throw new InvalidOperationException("Користувач з таким ім'ям вже існує");
                }
                user.UserName = userName;
                hasChanges = true;
            }

            if (avatarFile != null)
            {
                long maxFileSize = 5 * 1024 * 1024; // 5 MB
                if (avatarFile.Length > maxFileSize)
                {
                    throw new InvalidOperationException("Файл занадто великий. Максимальний розмір — 5 МБ");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(avatarFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException("Недопустимий формат файлу. Дозволені: .jpg, .jpeg, .png, .webp");
                }

                // Delete old avatar if it exists
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    await deleteOldAvatarAsync(user.AvatarUrl);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var cloudinaryUrl = await uploadToCloudinaryAsync(avatarFile, fileName);
                user.AvatarUrl = cloudinaryUrl;
                hasChanges = true;
            }
            else if (removeAvatar)
            {
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    await deleteOldAvatarAsync(user.AvatarUrl);
                    user.AvatarUrl = null;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Database error while updating profile for user {UserId}: {Errors}", id, errors);
                    throw new InvalidOperationException("Не вдалося оновити профіль: " + errors);
                }

                _logger.LogInformation("User profile {UserId} successfully updated.", id);
            }
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
                    _logger.LogError("Cloudinary configuration is missing.");
                    throw new InvalidOperationException("Cloudinary не налаштовано.");
                }

                var account = new Account(cloudName, apiKey, apiSecret);
                var cloudinary = new Cloudinary(account);

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = "avatars",
                    // Automatic compression, quality adjustment, and modern format selection (webp/avif)
                    Transformation = new Transformation()
                        .Quality("auto:good")
                        .FetchFormat("auto"),
                    UseFilename = true,
                    UniqueFilename = false
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary API error during upload: {Error}", uploadResult.Error.Message);
                    throw new InvalidOperationException($"Помилка завантаження зображення: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl?.ToString()
                       ?? throw new InvalidOperationException("Не вдалося отримати URL з Cloudinary.");
            }
            catch (Exception ex) when (ex is not InvalidOperationException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Unexpected error while interacting with Cloudinary.");
                throw new InvalidOperationException("Помилка завантаження в Cloudinary.", ex);
            }
        }

        private async Task deleteOldAvatarAsync(string oldAvatarUrl)
        {
            try
            {
                // Check if it is a Cloudinary URL
                if (!oldAvatarUrl.Contains("cloudinary.com"))
                {
                    return; // Not a Cloudinary file, do not delete
                }

                // Extract public ID from Cloudinary URL
                var publicId = extractPublicIdFromUrl(oldAvatarUrl);
                if (string.IsNullOrEmpty(publicId))
                {
                    return; // Failed to extract ID, skip
                }

                // Cloudinary configuration
                var cloudName = _configuration["Cloudinary:CloudName"];
                var apiKey = _configuration["Cloudinary:ApiKey"];
                var apiSecret = _configuration["Cloudinary:ApiSecret"];

                if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                {
                    return; // Cloudinary not configured, do not delete
                }

                var account = new Account(cloudName, apiKey, apiSecret);
                var cloudinary = new Cloudinary(account);

                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.Result != "ok")
                {
                    _logger.LogWarning("Failed to delete old avatar from Cloudinary. Result: {Result}", deletionResult.Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred while deleting old avatar.");
            }
        }
        private string? extractPublicIdFromUrl(string cloudinaryUrl)
        {
            try
            {
                var uri = new Uri(cloudinaryUrl);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                // Find the index of the "upload" segment
                int uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex == -1 || uploadIndex >= segments.Length - 1)
                {
                    _logger.LogWarning("Failed to extract public ID: 'upload' segment not found in URL: {Url}", cloudinaryUrl);
                    return null;
                }

                // Skip "upload" and any version (starts with 'v' followed by numbers) or transformation segments
                int startIndex = uploadIndex + 1;
                List<string> pathParts = new List<string>();

                for (int i = startIndex; i < segments.Length; i++)
                {
                    string segment = segments[i];

                    // Skip version segment (e.g., v1787773014)
                    if (i == startIndex && segment.StartsWith("v") && segment.Length > 1 && long.TryParse(segment.Substring(1), out _))
                    {
                        continue;
                    }

                    // Add valid path parts (e.g., "avatars" and the file name)
                    pathParts.Add(segment);
                }

                if (pathParts.Count == 0)
                {
                    _logger.LogWarning("Failed to extract public ID: no path parts found after parsing URL: {Url}", cloudinaryUrl);
                    return null;
                }

                // Combine back into "avatars/filename" format and strip the file extension
                var fullPath = string.Join("/", pathParts);
                int lastDot = fullPath.LastIndexOf('.');
                if (lastDot > 0)
                {
                    fullPath = fullPath.Substring(0, lastDot);
                }

                _logger.LogInformation("Successfully extracted Cloudinary Public ID: {PublicId} from URL: {Url}", fullPath, cloudinaryUrl);
                return fullPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while extracting public ID from Cloudinary URL: {Url}", cloudinaryUrl);
                return null;
            }
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
    }
}
