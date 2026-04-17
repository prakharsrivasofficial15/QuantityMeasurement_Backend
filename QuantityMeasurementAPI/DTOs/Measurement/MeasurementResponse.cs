namespace QuantityMeasurementAPI.DTOs.Measurement
{
    public class MeasurementResponse
    {
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsEqual { get; set; }  // For comparison
        public string? Error { get; set; }
        public bool Success { get; set; } = true;
    }
}