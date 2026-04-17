using System.Security.Claims;
using QuantityMeasurementAPI.Entities;

namespace QuantityMeasurementAPI.Services.Auth
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}