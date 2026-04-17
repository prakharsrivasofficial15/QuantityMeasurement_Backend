namespace ModelLayer.DTOs
{
    public class MeasurementRequest
    {
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // LENGTH, WEIGHT, VOLUME, TEMPERATURE
        
        public bool IsValid() => 
            !double.IsNaN(Value) && 
            !double.IsInfinity(Value) && 
            !string.IsNullOrWhiteSpace(Unit) && 
            !string.IsNullOrWhiteSpace(Type);
    }
}