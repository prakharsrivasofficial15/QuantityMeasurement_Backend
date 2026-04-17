namespace ModelLayer.DTOs
{
    public class MeasurementRecord
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public string Operation { get; }
        public MeasurementRequest? Operand1 { get; }
        public MeasurementRequest? Operand2 { get; }
        public object? Result { get; }
        public bool HasError { get; }
        public string? ErrorMessage { get; }

        public MeasurementRecord(string operation, MeasurementRequest op1, object result) 
            : this(operation, op1, null, result, false, null) { }
            
        public MeasurementRecord(string operation, MeasurementRequest op1, MeasurementRequest op2, object result) 
            : this(operation, op1, op2, result, false, null) { }
            
        public MeasurementRecord(string operation, MeasurementRequest op1, string error) 
            : this(operation, op1, null, null, true, error) { }
            
        public MeasurementRecord(string operation, MeasurementRequest op1, MeasurementRequest op2, string error) 
            : this(operation, op1, op2, null, true, error) { }
            
        private MeasurementRecord(string operation, MeasurementRequest? op1, MeasurementRequest? op2, 
                                  object? result, bool hasError, string? error)
        {
            Operation = operation;
            Operand1 = op1;
            Operand2 = op2;
            Result = result;
            HasError = hasError;
            ErrorMessage = error;
        }
    }
}