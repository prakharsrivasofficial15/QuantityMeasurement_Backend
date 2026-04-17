using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementAPI.DTOs.Auth;
using QuantityMeasurementAPI.Services.Auth;

namespace QuantityMeasurementAPI.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related API endpoints like register, login, and getting current user info.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Constructor that injects the authentication service.
        /// </summary>
        /// <param name="authService">The service handling authentication logic.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Endpoint for user registration.
        /// </summary>
        /// <param name="request">The registration request containing user details.</param>
        /// <returns>Success message or error if registration fails.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _authService.Register(request);
            
            if (user == null)
                return BadRequest(new { message = "Username or email already exists" });

            return Ok(new 
            { 
                message = "Registration successful", 
                username = user.Username,
                email = user.Email 
            });
        }

        /// <summary>
        /// Endpoint for user login.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>Login response with token or error if login fails.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.Login(request);
            
            if (response == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(response);
        }

        /// <summary>
        /// Endpoint to get information about the currently authenticated user.
        /// Requires authorization.
        /// </summary>
        /// <returns>Current user details.</returns>
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name;
            
            return Ok(new 
            { 
                userId, 
                username,
                message = "You are authenticated!" 
            });
        }
    }
}