using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using QuantityMeasurementAPI.Data;
using QuantityMeasurementAPI.DTOs.Auth;
using QuantityMeasurementAPI.DTOs.User;
using QuantityMeasurementAPI.Entities;
using QuantityMeasurementAPI.Exceptions;

namespace QuantityMeasurementAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context, 
            IConfiguration configuration, 
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        #region Registration & Login

        public async Task<User?> Register(RegisterRequest request)
        {
            // Check if user exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
            
            if (existingUser != null)
                return null;

            // Generate salt and hash
            var salt = GenerateSalt();
            var hash = HashPassword(request.Password, salt);

            // Create user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hash,
                Salt = salt,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsGoogleUser = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);
            
            if (user == null || !user.IsActive)
                return null;

            // Don't allow Google users to login with password
            if (user.IsGoogleUser)
                return null;

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash ?? "", user.Salt ?? ""))
                return null;

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "60"))
            };
        }

        #endregion

        #region Google Authentication

        public async Task<GoogleAuthResponse?> GoogleLogin(string idToken)
        {
            try
            {
                // Verify the Google token
                var googleUser = await VerifyGoogleToken(idToken);
                if (googleUser == null || !googleUser.EmailVerified)
                {
                    _logger.LogWarning("Invalid Google token or unverified email");
                    return null;
                }

                // Check if user exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == googleUser.Email || u.GoogleId == googleUser.Id);

                bool isNewUser = false;

                if (user == null)
                {
                    // Create new user
                    isNewUser = true;
                    user = new User
                    {
                        Username = googleUser.Email.Split('@')[0],
                        Email = googleUser.Email,
                        GoogleId = googleUser.Id,
                        Picture = googleUser.Picture,
                        GivenName = googleUser.GivenName,
                        FamilyName = googleUser.FamilyName,
                        IsGoogleUser = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    // Ensure username is unique
                    var baseUsername = user.Username;
                    var counter = 1;
                    while (await _context.Users.AnyAsync(u => u.Username == user.Username))
                    {
                        user.Username = $"{baseUsername}{counter}";
                        counter++;
                    }
                    
                    _context.Users.Add(user);
                }
                else if (string.IsNullOrEmpty(user.GoogleId))
                {
                    // Existing local user - link Google account
                    user.GoogleId = googleUser.Id;
                    user.Picture = googleUser.Picture;
                    user.IsGoogleUser = true;
                    _context.Users.Update(user);
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(user);

                return new GoogleAuthResponse
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Picture = user.Picture,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "60")),
                    IsNewUser = isNewUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google login failed");
                return null;
            }
        }

        public async Task<GoogleUserInfo?> VerifyGoogleToken(string idToken)
        {
            try
            {
                // Google's token verification endpoint
                var response = await _httpClient.GetAsync(
                    $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google token verification failed: {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<GoogleUserInfo>(json);
                
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Google token");
                return null;
            }
        }

        #endregion

        #region User Management

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> UpdateUser(int id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var existing = await _context.Users
                    .AnyAsync(u => u.Username == request.Username && u.Id != id);
                if (existing) 
                    throw new ValidationException("Username is already taken.");
                user.Username = request.Username;
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existing = await _context.Users
                    .AnyAsync(u => u.Email == request.Email && u.Id != id);
                if (existing) 
                    throw new ValidationException("Email is already in use.");
                user.Email = request.Email;
            }

            if (!user.IsGoogleUser && !string.IsNullOrWhiteSpace(request.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                    !VerifyPassword(request.CurrentPassword, user.PasswordHash ?? "", user.Salt ?? ""))
                    throw new ValidationException("Current password is incorrect.");

                var salt = GenerateSalt();
                user.PasswordHash = HashPassword(request.NewPassword, salt);
                user.Salt = salt;
            }

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region JWT & Password Helpers

        public bool VerifyPassword(string password, string hash, string salt)
        {
            var computedHash = HashPassword(password, salt);
            return computedHash == hash;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "DefaultKey32CharactersLongEnoughForDevelopment!");
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("isGoogleUser", user.IsGoogleUser.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "60")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }

        #endregion
    }
}