using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Backend.Interfaces;
using Backend.Models;
using System.Threading.Tasks;
using Backend.Models.Dtos;

namespace Backend.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(  UserManager<AppUser> userManager,
                             RoleManager<AppRole> roleManager,
                             IHttpContextAccessor httpContextAccessor,
                             ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

        }

        public async Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return Result<IEnumerable<UserDto>>.SuccessResult(userDtos);
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<UserDto>.Failure("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            };

            return Result<UserDto>.SuccessResult(userDto);
        }

        public async Task<Result> UpdateUserAsync(UserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id);
            if (user == null)
                return Result.Failure("User not found.");

            user.UserName = userDto.UserName;
            user.Email = userDto.Email;
            user.LastModified = DateTime.UtcNow;
            user.LastModifiedBy = GetCurrentUserName();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to update user: {errors}");
            }

            return Result.SuccessResult("User updated successfully");
        }
        public async Task<Result> AssignRoleAsync(AssignRoleDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return Result.Failure("User not found.");

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                var newRole = new AppRole(model.Role);
                var roleResult = await _roleManager.CreateAsync(newRole);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return Result.Failure($"Failed to create role: {errors}");
                }
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Contains(model.Role))
                return Result.Failure("User already has this role.");

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to assign role: {errors}");
            }

            return Result.SuccessResult("Role assigned successfully");
        }

        private string GetCurrentUserName()
        {
                return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        }

        // Additional user-related methods can be added here
    }
}