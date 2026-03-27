using Core.DTOs;
using Microsoft.AspNetCore.Identity;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterDto dto);
    Task<string> LoginAsync(LoginDto dto);
}