namespace BusinessLayer.Exceptions
{
    public class DatabaseException : QuantityMeasurementException
    {
        public DatabaseException() { }
        public DatabaseException(string message) : base(message) { }
        public DatabaseException(string message, Exception inner) : base(message, inner) { }
    }
}