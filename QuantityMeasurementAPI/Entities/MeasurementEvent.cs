namespace QuantityMeasurementAPI.Entities
{
    public class MeasurementEvent
    {
        public string Operation { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}