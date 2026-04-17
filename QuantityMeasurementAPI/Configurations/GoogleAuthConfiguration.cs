namespace QuantityMeasurementAPI.Configurations
{
    public class GoogleAuthConfiguration
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string UserInfoEndpoint { get; set; } = "https://www.googleapis.com/oauth2/v2/userinfo";
        public string TokenEndpoint { get; set; } = "https://oauth2.googleapis.com/token";
    }
}
