using System;

namespace QuantityMeasurementAPI.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException() : base() { }
        
        public BusinessException(string message) : base(message) { }
        
        public BusinessException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}