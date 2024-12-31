using Backend.Models.Dtos;

namespace Backend.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDto model);
        Task<bool> SignUpAsync(RegisterDto registerDto);
        // More identity/auth-related methods...
    }
}
