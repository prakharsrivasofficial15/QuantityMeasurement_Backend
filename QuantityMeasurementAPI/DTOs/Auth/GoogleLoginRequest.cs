using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAPI.DTOs.Auth
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}