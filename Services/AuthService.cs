using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.DTOs;
using Core.Models;
using Core.Models.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<User> userManager,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleNames.User);
            }
            
            return result;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.UserNameOrEmail);
            if(user == null)
                user = await _userManager.FindByNameAsync(dto.UserNameOrEmail);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Неправильний логін або пароль");

            return await GenerateJwtToken(user);
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, string.Join(",", await _userManager.GetRolesAsync(user)))
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<string> ExternalLoginAsync(string email, string name, string avatarUrl, string googleId)
        {
            var user = await _userManager.FindByLoginAsync("Google", googleId);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = email.Split('@')[0],
                        Email = email,
                        AvatarUrl = avatarUrl,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        throw new Exception(string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
                if (!addLoginResult.Succeeded)
                    throw new Exception("Не вдалося прив'язати Google-акаунт");
            }
            else
            {
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
            }

            if (!string.IsNullOrEmpty(avatarUrl) && user.AvatarUrl != avatarUrl)
            {
                user.AvatarUrl = avatarUrl;
                await _userManager.UpdateAsync(user);
            }

            return await GenerateJwtToken(user);
        }
    }
}

