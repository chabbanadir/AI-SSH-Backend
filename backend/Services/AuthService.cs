using Microsoft.AspNetCore.Identity;
using Backend.Interfaces;
using Backend.Models.Dtos; // for RegisterDto or similar
using System.Threading.Tasks;
using Backend.Models;
namespace Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
         private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<string?> LoginAsync(LoginDto model)
        {
            // 1) Find user
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) 
                return null; // or throw new UnauthorizedAccessException();

            // 2) Check password
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
                return null; // or throw new UnauthorizedAccessException();

            // 3) Retrieve roles
            var roles = await _userManager.GetRolesAsync(user);

            // 4) Generate JWT
            return _tokenService.GenerateToken(user, roles);
        }

        public async Task<bool> SignUpAsync(RegisterDto registerDto)
        {
            var user = new AppUser {UserName = registerDto.UserName, 
                                    Email = registerDto.Email,
                                    SshClient = registerDto.SshClient
                                     };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            return result.Succeeded;
        }
    }
}
