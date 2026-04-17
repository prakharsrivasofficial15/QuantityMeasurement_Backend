using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAPI.DTOs.Measurement
{
    public class QuantityInputDTO
    {
        [Required]
        public double Value { get; set; }
        
        [Required]
        [RegularExpression("^(FEET|INCHES|YARDS|CENTIMETERS|KILOGRAM|GRAM|POUND|TONNE|MILLIGRAM|LITRE|MILLILITRE|GALLON|CELSIUS|FAHRENHEIT)$", 
            ErrorMessage = "Invalid unit")]
        public string Unit { get; set; } = string.Empty;
        
        [Required]
        [RegularExpression("^(LENGTH|WEIGHT|VOLUME|TEMPERATURE)$", 
            ErrorMessage = "Invalid measurement type")]
        public string Type { get; set; } = string.Empty;
    }
}