using Backend.Models.Dtos;
namespace Backend.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserDto>> GetUserByIdAsync(string userId);
        Task<Result> UpdateUserAsync(UserDto userDto);
        Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<Result> AssignRoleAsync(AssignRoleDto model);
    }
}
