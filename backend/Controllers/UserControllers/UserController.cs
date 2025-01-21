using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Models.Dtos;
using Backend.Interfaces;

namespace Backend.Controllers.UserControllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public UserController(
            IAuthService authService,
            IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _authService.SignUpAsync(model);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }

        // Login a user
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _authService.LoginAsync(model);
            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(new { message = result });
        }

        // Logout a user
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync();
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }

        // Get all users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsersAsync();
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        // Assign a role to a user
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            var result = await _userService.AssignRoleAsync(model);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }

        // Get user by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        // Update user
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
                return BadRequest("User ID mismatch");

            var result = await _userService.UpdateUserAsync(userDto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }
    }
}