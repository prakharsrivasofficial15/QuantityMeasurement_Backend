using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using QuantityMeasurementAPI.Configurations;
using QuantityMeasurementAPI.DTOs.Auth;

namespace QuantityMeasurementAPI.Services.Auth
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleAuthConfiguration _googleAuthConfig;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _httpClient = new HttpClient();
            _googleAuthConfig = configuration.GetSection("Google").Get<GoogleAuthConfiguration>() ?? new GoogleAuthConfiguration();
            _logger = logger;
        }

        public async Task<GoogleUserInfo?> VerifyGoogleToken(string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google token verification failed: {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Google token");
                return null;
            }
        }

        public async Task<GoogleAuthResponse?> AuthenticateWithGoogle(string idToken)
        {
            try
            {
                var userInfo = await VerifyGoogleToken(idToken);
                if (userInfo == null || !userInfo.EmailVerified)
                {
                    _logger.LogWarning("Invalid Google token or unverified email");
                    return null;
                }

                // In a real implementation, you would:
                // 1. Check if user exists in your database
                // 2. Create or update user record
                // 3. Generate JWT token
                // 4. Return authentication response

                // For now, return a basic response structure
                return new GoogleAuthResponse
                {
                    Token = idToken, // This would be your JWT token in real implementation
                    Username = userInfo.Email?.Split('@')[0] ?? string.Empty,
                    Email = userInfo.Email ?? string.Empty,
                    Picture = userInfo.Picture,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    IsNewUser = false // This would be determined by checking your database
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google authentication failed");
                return null;
            }
        }
    }
}
