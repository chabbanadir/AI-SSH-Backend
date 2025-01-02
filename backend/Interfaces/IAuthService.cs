using Backend.Models.Dtos;

namespace Backend.Interfaces
{
    public interface IAuthService
    {
        Task<Result<string>> LoginAsync(LoginDto model);
        Task<Result> SignUpAsync(RegisterDto registerDto);
        Task<Result> LogoutAsync();
    }
}
