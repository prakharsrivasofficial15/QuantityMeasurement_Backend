using ModelLayer.DTOs;
using System.Collections.Generic;

namespace RepositoryLayer.Interfaces
{
    public interface IQuantityMeasurementRepository
    {
        void Save(MeasurementRecord record);
        IEnumerable<MeasurementRecord> GetAll();
        void Clear();
    }
}