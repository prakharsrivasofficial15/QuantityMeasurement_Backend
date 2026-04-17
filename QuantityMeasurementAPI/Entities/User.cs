using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAPI.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public string Salt { get; set; } = string.Empty;
        
        // Google OAuth fields
        public string? GoogleId { get; set; }
        public string? Picture { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsGoogleUser { get; set; } = false;
    }
}