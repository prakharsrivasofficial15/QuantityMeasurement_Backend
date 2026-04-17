using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAPI.DTOs.Measurement
{
    public class ConvertRequest
    {
        [Required]
        public QuantityInputDTO Quantity { get; set; } = new();
        
        [Required]
        public string TargetUnit { get; set; } = string.Empty;
    }
}