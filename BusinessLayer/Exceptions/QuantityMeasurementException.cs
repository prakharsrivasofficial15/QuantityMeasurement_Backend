namespace BusinessLayer.Exceptions
{
    public class QuantityMeasurementException : Exception
    {
        public QuantityMeasurementException() { }
        public QuantityMeasurementException(string message) : base(message) { }
        public QuantityMeasurementException(string message, Exception inner) : base(message, inner) { }
    }
}