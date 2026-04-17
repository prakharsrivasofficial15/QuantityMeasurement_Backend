using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementAPI.DTOs.Auth;
using QuantityMeasurementAPI.DTOs.User;
using QuantityMeasurementAPI.Services.Auth;
using System.Security.Claims;

namespace QuantityMeasurementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(IAuthService authService, ILogger<UserController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _authService.GetUserById(int.Parse(userId));
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Picture,
                user.IsGoogleUser,
                user.CreatedAt,
                user.LastLoginAt
            });
        }

        /// <summary>
        /// Update current user profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _authService.UpdateUser(int.Parse(userId), request);

            return Ok(new
            {
                user!.Username,
                user.Email,
                message = "Profile updated successfully"
            });
        }

        /// <summary>
        /// Delete current user account
        /// </summary>
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _authService.DeleteUser(int.Parse(userId));
            if (!result)
                return NotFound();

            return Ok(new { message = "Account deleted successfully" });
        }
    }
}