using Microsoft.AspNetCore.Identity;
using Backend.Interfaces;
using Backend.Models.Dtos;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Models.Entities; // Ensure this namespace includes AppUser
using Microsoft.AspNetCore.Http;
namespace Backend.Services.Security
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<string>> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return Result<string>.Failure("Invalid username or password.");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
                return Result<string>.Failure("Invalid username or password.");

            // Optionally, you can store additional session data here if needed
            // e.g., _httpContextAccessor.HttpContext.Session.SetString("UserId", user.Id);

            return Result<string>.SuccessResult("Login successful");
        }

        public async Task<Result> SignUpAsync(RegisterDto registerDto)
        {
            var userExists = await _userManager.FindByNameAsync(registerDto.UserName);
            if (userExists != null)
                return Result.Failure("Username already exists.");

            var user = new AppUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Created = DateTime.UtcNow,
                CreatedBy = registerDto.UserName // Assuming the user is registering themselves
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"User registration failed: {errors}");
            }

            // Optionally, assign a default role
            // await _userManager.AddToRoleAsync(user, "User");

            return Result.SuccessResult("User registered successfully");
        }

        public async Task<Result> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            // Optionally, clear additional session data
            //_httpContextAccessor.HttpContext.Session.Clear();
            return Result.SuccessResult("Logged out successfully");
        }
    }
}