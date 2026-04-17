using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAPI.DTOs.User
{
    public class UpdateUserRequest
    {
        [MinLength(3)]
        [MaxLength(50)]
        public string? Username { get; set; }
        
        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }
        
        [MinLength(6)]
        public string? CurrentPassword { get; set; }
        
        [MinLength(6)]
        public string? NewPassword { get; set; }
    }
}