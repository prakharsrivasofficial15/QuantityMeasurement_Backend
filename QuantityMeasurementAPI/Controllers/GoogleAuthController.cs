using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementAPI.DTOs.Auth;
using QuantityMeasurementAPI.Services.Auth;
using Microsoft.Extensions.Configuration;

namespace QuantityMeasurementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<GoogleAuthController> _logger;
        private readonly IConfiguration _configuration;

        public GoogleAuthController(
            IAuthService authService, 
            ILogger<GoogleAuthController> logger,
            IConfiguration configuration) 
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;  //Initialize configuration
        }

        /// <summary>
        /// Login with Google ID token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.GoogleLogin(request.IdToken);
            
            if (response == null)
                return Unauthorized(new { message = "Invalid Google token" });

            return Ok(response);
        }

        /// <summary>
        /// Get Google OAuth URL for web login
        /// </summary>
        [HttpGet("url")]
        public IActionResult GetGoogleAuthUrl()
        {
            var clientId = _configuration["Google:ClientId"];
            var redirectUri = _configuration["Google:RedirectUri"];
            
            // Validate configuration
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            {
                _logger.LogError("Google OAuth configuration missing: ClientId or RedirectUri");
                return StatusCode(500, new { message = "Google authentication is not properly configured" });
            }
            
            var url = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                      $"client_id={clientId}&" +
                      $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" + 
                      $"response_type=code&" +
                      $"scope=email%20profile%20openid&" +
                      $"access_type=offline";
            
            return Ok(new { url });
        }
    }
}