using Backend.Models.Dtos;
namespace Backend.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(UserDto userDto);
        // Additional user-related methods
    }
}
