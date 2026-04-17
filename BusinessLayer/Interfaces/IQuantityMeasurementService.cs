using ModelLayer.DTOs;

namespace BusinessLayer.Interfaces
{
    public interface IQuantityMeasurementService
    {
        // Core operations - using simplified DTOs
        MeasurementRecord Compare(MeasurementRequest request1, MeasurementRequest request2);
        MeasurementRecord Convert(MeasurementRequest request, string targetUnit);
        MeasurementRecord Add(MeasurementRequest request1, MeasurementRequest request2);
        MeasurementRecord Subtract(MeasurementRequest request1, MeasurementRequest request2);
        MeasurementRecord Divide(MeasurementRequest request1, MeasurementRequest request2);
        
        // For retrieving history
        IEnumerable<MeasurementRecord> GetHistory();
    }
}