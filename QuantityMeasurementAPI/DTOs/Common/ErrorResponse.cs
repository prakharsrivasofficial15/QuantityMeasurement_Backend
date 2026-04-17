namespace QuantityMeasurementAPI.DTOs.Common
{
    public class ErrorResponse
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Path { get; set; }
        public string? Details { get; set; }
    }
}