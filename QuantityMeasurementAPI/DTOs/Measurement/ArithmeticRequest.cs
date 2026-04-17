using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAPI.DTOs.Measurement
{
    public class ArithmeticRequest
    {
        [Required]
        public QuantityInputDTO Quantity1 { get; set; } = new();
        
        [Required]
        public QuantityInputDTO Quantity2 { get; set; } = new();
        
        public string? TargetUnit { get; set; }  // Optional - for result unit
    }
}