using QuantityMeasurementAPI.DTOs.Auth;

namespace QuantityMeasurementAPI.Services.Auth
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo?> VerifyGoogleToken(string idToken);
        Task<GoogleAuthResponse?> AuthenticateWithGoogle(string idToken);
    }
}
