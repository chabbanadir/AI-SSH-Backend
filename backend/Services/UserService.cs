using Microsoft.AspNetCore.Identity;
using Backend.Interfaces;
using Backend.Models;
using System.Threading.Tasks;
using Backend.Models.Dtos;

namespace Backend.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        
        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserDto 
            { 
                UserId = user.Id, 
                UserName = user.UserName, 
                Email = user.Email 
            };
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.UserId);
            if (user != null)
            {
                user.UserName = userDto.UserName;
                user.Email = userDto.Email;
                await _userManager.UpdateAsync(user);
            }
        }
    }
}
