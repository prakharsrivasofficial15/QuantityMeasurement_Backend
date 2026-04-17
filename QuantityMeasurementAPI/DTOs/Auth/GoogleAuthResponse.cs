namespace QuantityMeasurementAPI.DTOs.Auth
{
    public class GoogleAuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsNewUser { get; set; }
    }
}