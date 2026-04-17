using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using ModelLayer.DTOs;
using ModelLayer.Entities;
using ModelLayer.Enums;
using RepositoryLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLayer.Services
{
    public class QuantityMeasurementService : IQuantityMeasurementService
    {
        private readonly IQuantityMeasurementRepository _repository;

        public QuantityMeasurementService(IQuantityMeasurementRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        #region Public Methods
        
        public MeasurementRecord Compare(MeasurementRequest request1, MeasurementRequest request2)
        {
            try
            {
                ValidateRequests(request1, request2);
                
                bool isEqual = false;
                
                switch (request1.Type)
                {
                    case "LENGTH":
                        var len1 = new Quantity<LengthUnit>(request1.Value, Enum.Parse<LengthUnit>(request1.Unit));
                        var len2 = new Quantity<LengthUnit>(request2.Value, Enum.Parse<LengthUnit>(request2.Unit));
                        isEqual = len1.Equals(len2);
                        break;
                        
                    case "WEIGHT":
                        var wt1 = new Quantity<WeightUnit>(request1.Value, Enum.Parse<WeightUnit>(request1.Unit));
                        var wt2 = new Quantity<WeightUnit>(request2.Value, Enum.Parse<WeightUnit>(request2.Unit));
                        isEqual = wt1.Equals(wt2);
                        break;
                        
                    case "VOLUME":
                        var vol1 = new Quantity<VolumeUnit>(request1.Value, Enum.Parse<VolumeUnit>(request1.Unit));
                        var vol2 = new Quantity<VolumeUnit>(request2.Value, Enum.Parse<VolumeUnit>(request2.Unit));
                        isEqual = vol1.Equals(vol2);
                        break;
                        
                    case "TEMPERATURE":
                        var temp1 = new Quantity<TemperatureUnit>(request1.Value, Enum.Parse<TemperatureUnit>(request1.Unit));
                        var temp2 = new Quantity<TemperatureUnit>(request2.Value, Enum.Parse<TemperatureUnit>(request2.Unit));
                        isEqual = temp1.Equals(temp2);
                        break;
                        
                    default:
                        throw new QuantityMeasurementException($"Unsupported measurement type: {request1.Type}");
                }
                
                var resultDto = CreateScalarResult(isEqual ? 1 : 0, "BOOLEAN");
                var record = new MeasurementRecord("COMPARE", request1, request2, resultDto);
                _repository.Save(record);
                
                return record;
            }
            catch (Exception ex) when (!(ex is QuantityMeasurementException))
            {
                var errorResult = CreateErrorResult();
                var errorRecord = new MeasurementRecord("COMPARE", request1, request2, errorResult);
                _repository.Save(errorRecord);
                throw new QuantityMeasurementException($"Comparison failed: {ex.Message}", ex);
            }
        }

        public MeasurementRecord Convert(MeasurementRequest request, string targetUnit)
        {
            try
            {
                ValidateRequest(request);
                
                if (string.IsNullOrWhiteSpace(targetUnit))
                    throw new QuantityMeasurementException("Target unit cannot be null or empty");
                
                MeasurementRequest? resultDto = null;
                
                // Handle each measurement type separately
                switch (request.Type)
                {
                    case "LENGTH":
                        if (!Enum.TryParse<LengthUnit>(targetUnit, out var targetLenUnit))
                            throw new QuantityMeasurementException($"Invalid target unit '{targetUnit}' for LENGTH");
                        var lenQuantity = new Quantity<LengthUnit>(request.Value, Enum.Parse<LengthUnit>(request.Unit));
                        var convertedLen = lenQuantity.ConvertTo(targetLenUnit);
                        resultDto = new MeasurementRequest
                        {
                            Value = Math.Round(convertedLen.Value, 5),
                            Unit = convertedLen.Unit.ToString(),
                            Type = request.Type
                        };
                        break;
                        
                    case "WEIGHT":
                        if (!Enum.TryParse<WeightUnit>(targetUnit, out var targetWtUnit))
                            throw new QuantityMeasurementException($"Invalid target unit '{targetUnit}' for WEIGHT");
                        var wtQuantity = new Quantity<WeightUnit>(request.Value, Enum.Parse<WeightUnit>(request.Unit));
                        var convertedWt = wtQuantity.ConvertTo(targetWtUnit);
                        resultDto = new MeasurementRequest
                        {
                            Value = Math.Round(convertedWt.Value, 5),
                            Unit = convertedWt.Unit.ToString(),
                            Type = request.Type
                        };
                        break;
                        
                    case "VOLUME":
                        if (!Enum.TryParse<VolumeUnit>(targetUnit, out var targetVolUnit))
                            throw new QuantityMeasurementException($"Invalid target unit '{targetUnit}' for VOLUME");
                        var volQuantity = new Quantity<VolumeUnit>(request.Value, Enum.Parse<VolumeUnit>(request.Unit));
                        var convertedVol = volQuantity.ConvertTo(targetVolUnit);
                        resultDto = new MeasurementRequest
                        {
                            Value = Math.Round(convertedVol.Value, 5),
                            Unit = convertedVol.Unit.ToString(),
                            Type = request.Type
                        };
                        break;
                        
                    case "TEMPERATURE":
                        if (!Enum.TryParse<TemperatureUnit>(targetUnit, out var targetTempUnit))
                            throw new QuantityMeasurementException($"Invalid target unit '{targetUnit}' for TEMPERATURE");
                        var tempQuantity = new Quantity<TemperatureUnit>(request.Value, Enum.Parse<TemperatureUnit>(request.Unit));
                        var convertedTemp = tempQuantity.ConvertTo(targetTempUnit);
                        resultDto = new MeasurementRequest
                        {
                            Value = Math.Round(convertedTemp.Value, 5),
                            Unit = convertedTemp.Unit.ToString(),
                            Type = request.Type
                        };
                        break;
                        
                    default:
                        throw new QuantityMeasurementException($"Unsupported measurement type: {request.Type}");
                }
                
                var record = new MeasurementRecord("CONVERT", request, resultDto);
                _repository.Save(record);
                
                return record;
            }
            catch (Exception ex) when (!(ex is QuantityMeasurementException))
            {
                var errorResult = CreateErrorResult();
                var errorRecord = new MeasurementRecord("CONVERT", request, errorResult);
                _repository.Save(errorRecord);
                throw new QuantityMeasurementException($"Conversion failed: {ex.Message}", ex);
            }
        }

        public MeasurementRecord Add(MeasurementRequest request1, MeasurementRequest request2)
        {
            return PerformArithmetic(request1, request2, "ADD");
        }

        public MeasurementRecord Subtract(MeasurementRequest request1, MeasurementRequest request2)
        {
            return PerformArithmetic(request1, request2, "SUBTRACT");
        }

        public MeasurementRecord Divide(MeasurementRequest request1, MeasurementRequest request2)
        {
            try
            {
                ValidateRequests(request1, request2);
                
                if (request1.Type != request2.Type)
                {
                    throw new QuantityMeasurementException(
                        $"Cannot divide different measurement types: {request1.Type} and {request2.Type}");
                }
                
                // Temperature cannot be divided
                if (request1.Type == "TEMPERATURE")
                {
                    throw new QuantityMeasurementException("Temperature division is not supported");
                }
                
                double result = 0;
                
                // Handle each measurement type separately
                switch (request1.Type)
                {
                    case "LENGTH":
                        var len1 = new Quantity<LengthUnit>(request1.Value, Enum.Parse<LengthUnit>(request1.Unit));
                        var len2 = new Quantity<LengthUnit>(request2.Value, Enum.Parse<LengthUnit>(request2.Unit));
                        if (Math.Abs(len2.Value) < 0.000001)
                            throw new QuantityMeasurementException("Cannot divide by zero");
                        result = len1.Divide(len2);
                        break;

                    case "WEIGHT":
                        var wt1 = new Quantity<WeightUnit>(request1.Value, Enum.Parse<WeightUnit>(request1.Unit));
                        var wt2 = new Quantity<WeightUnit>(request2.Value, Enum.Parse<WeightUnit>(request2.Unit));
                        if (Math.Abs(wt2.Value) < 0.000001)
                            throw new QuantityMeasurementException("Cannot divide by zero");
                        result = wt1.Divide(wt2);
                        break;

                    case "VOLUME":
                        var vol1 = new Quantity<VolumeUnit>(request1.Value, Enum.Parse<VolumeUnit>(request1.Unit));
                        var vol2 = new Quantity<VolumeUnit>(request2.Value, Enum.Parse<VolumeUnit>(request2.Unit));
                        if (Math.Abs(vol2.Value) < 0.000001)
                            throw new QuantityMeasurementException("Cannot divide by zero");
                        result = vol1.Divide(vol2);
                        break;
                        
                    default:
                        throw new QuantityMeasurementException($"Unsupported measurement type: {request1.Type}");
                }
                
                var resultDto = CreateScalarResult(result, "SCALAR");
                
                var record = new MeasurementRecord("DIVIDE", request1, request2, resultDto);
                _repository.Save(record);
                
                return record;
            }
            catch (Exception ex) when (!(ex is QuantityMeasurementException))
            {
                var errorResult = CreateErrorResult();
                var errorRecord = new MeasurementRecord("DIVIDE", request1, request2, errorResult);
                _repository.Save(errorRecord);
                throw new QuantityMeasurementException($"Division failed: {ex.Message}", ex);
            }
        }

        public IEnumerable<MeasurementRecord> GetHistory()
        {
            return _repository.GetAll() ?? Enumerable.Empty<MeasurementRecord>();
        }

        #endregion

        #region Private Helper Methods

        private MeasurementRecord PerformArithmetic(MeasurementRequest request1, MeasurementRequest request2, string operation)
        {
            try
            {
                ValidateRequests(request1, request2);
                
                if (request1.Type != request2.Type)
                {
                    throw new QuantityMeasurementException(
                        $"Cannot {operation} different measurement types: {request1.Type} and {request2.Type}");
                }
                
                // Temperature cannot be added or subtracted
                if (request1.Type == "TEMPERATURE")
                {
                    throw new QuantityMeasurementException($"Temperature {operation} is not supported");
                }
                
                double result = 0;
                
                // Handle each measurement type separately
                switch (request1.Type)
                {
                    case "LENGTH":
                        var len1 = new Quantity<LengthUnit>(request1.Value, Enum.Parse<LengthUnit>(request1.Unit));
                        var len2 = new Quantity<LengthUnit>(request2.Value, Enum.Parse<LengthUnit>(request2.Unit));
                        result = operation == "ADD" ? len1.Add(len2).Value : len1.Subtract(len2).Value;
                        break;
                        
                    case "WEIGHT":
                        var wt1 = new Quantity<WeightUnit>(request1.Value, Enum.Parse<WeightUnit>(request1.Unit));
                        var wt2 = new Quantity<WeightUnit>(request2.Value, Enum.Parse<WeightUnit>(request2.Unit));
                        result = operation == "ADD" ? wt1.Add(wt2).Value : wt1.Subtract(wt2).Value;
                        break;
                        
                    case "VOLUME":
                        var vol1 = new Quantity<VolumeUnit>(request1.Value, Enum.Parse<VolumeUnit>(request1.Unit));
                        var vol2 = new Quantity<VolumeUnit>(request2.Value, Enum.Parse<VolumeUnit>(request2.Unit));
                        result = operation == "ADD" ? vol1.Add(vol2).Value : vol1.Subtract(vol2).Value;
                        break;
                        
                    default:
                        throw new QuantityMeasurementException($"Unsupported measurement type: {request1.Type}");
                }
                
                var resultDto = new MeasurementRequest
                {
                    Value = Math.Round(result, 5),
                    Unit = request1.Unit,
                    Type = request1.Type
                };
                
                var record = new MeasurementRecord(operation, request1, request2, resultDto);
                _repository.Save(record);
                
                return record;
            }
            catch (Exception ex) when (!(ex is QuantityMeasurementException))
            {
                var errorResult = CreateErrorResult();
                var errorRecord = new MeasurementRecord(operation, request1, request2, errorResult);
                _repository.Save(errorRecord);
                throw new QuantityMeasurementException($"{operation} failed: {ex.Message}", ex);
            }
        }

        private MeasurementRequest CreateScalarResult(double value, string unit)
        {
            return new MeasurementRequest
            {
                Value = Math.Round(value, 5),
                Unit = unit,
                Type = "RESULT"
            };
        }

        // Helper method to create error result
        private MeasurementRequest CreateErrorResult()
        {
            return new MeasurementRequest
            {
                Value = 0,
                Unit = "ERROR",
                Type = "ERROR",
            };
        }

        private void ValidateRequest(MeasurementRequest request)
        {
            if (request == null)
                throw new QuantityMeasurementException("Request cannot be null");
            
            if (!request.IsValid())
                throw new QuantityMeasurementException("Invalid request data");
        }

        private void ValidateRequests(MeasurementRequest request1, MeasurementRequest request2)
        {
            ValidateRequest(request1);
            ValidateRequest(request2);
        }

        #endregion
    }
}