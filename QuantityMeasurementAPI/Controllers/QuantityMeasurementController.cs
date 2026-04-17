using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using QuantityMeasurementAPI.DTOs.Measurement;
using QuantityMeasurementAPI.Exceptions;
using BusinessLayer.Interfaces;
using ModelLayer.DTOs;

namespace QuantityMeasurementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuantityMeasurementController : ControllerBase
    {
        private readonly IQuantityMeasurementService _service;
        private readonly IMemoryCache _cache;
        private readonly ILogger<QuantityMeasurementController> _logger;

        public QuantityMeasurementController(
            IQuantityMeasurementService service,
            IMemoryCache cache,
            ILogger<QuantityMeasurementController> logger)
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        #region Public APIs

        [HttpPost("compare")]
        public IActionResult Compare([FromBody] CompareRequest request)
        {
            ValidateCompareRequest(request);

            _logger.LogInformation("Comparing {V1}{U1} with {V2}{U2}",
                request.Quantity1.Value, request.Quantity1.Unit,
                request.Quantity2.Value, request.Quantity2.Unit);

            var q1 = Map(request.Quantity1);
            var q2 = Map(request.Quantity2);

            var record = _service.Compare(q1, q2);

            if (record.HasError)
                throw new BusinessException(record.ErrorMessage);

            bool isEqual = ExtractBooleanResult(record.Result);

            return Ok(new MeasurementResponse
            {
                Value = isEqual ? 1 : 0,
                Unit = "BOOLEAN",
                Type = "RESULT",
                IsEqual = isEqual,
                Success = true
            });
        }

        [HttpPost("convert")]
        public IActionResult Convert([FromBody] ConvertRequest request)
        {
            ValidateConvertRequest(request);

            string cacheKey = BuildCacheKey(request);

            if (_cache.TryGetValue(cacheKey, out MeasurementResponse cached))
            {
                _logger.LogInformation("Cache hit: {Key}", cacheKey);
                return Ok(cached);
            }

            _logger.LogInformation("Converting {Value}{Unit} to {Target}",
                request.Quantity.Value, request.Quantity.Unit, request.TargetUnit);

            var record = _service.Convert(Map(request.Quantity), request.TargetUnit);

            if (record.HasError)
                throw new BusinessException(record.ErrorMessage);

            var result = ExtractMeasurement(record.Result);

            var response = BuildResponse(result);

            _cache.Set(cacheKey, response, TimeSpan.FromMinutes(10));

            return Ok(response);
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] ArithmeticRequest request)
        {
            return ExecuteArithmetic(request, _service.Add, "Adding");
        }

        [HttpPost("subtract")]
        public IActionResult Subtract([FromBody] ArithmeticRequest request)
        {
            return ExecuteArithmetic(request, _service.Subtract, "Subtracting");
        }

        [HttpPost("divide")]
        public IActionResult Divide([FromBody] ArithmeticRequest request)
        {
            ValidateArithmeticRequest(request);

            _logger.LogInformation("Dividing {V1}{U1} by {V2}{U2}",
                request.Quantity1.Value, request.Quantity1.Unit,
                request.Quantity2.Value, request.Quantity2.Unit);

            try
            {
                var record = _service.Divide(
                    Map(request.Quantity1),
                    Map(request.Quantity2));

                if (record.HasError)
                    throw new BusinessException(record.ErrorMessage);

                var result = ExtractMeasurement(record.Result);

                return Ok(new MeasurementResponse
                {
                    Value = result.Value,
                    Unit = "SCALAR",
                    Type = "RESULT",
                    Success = true
                });
            }
            catch (DivideByZeroException)
            {
                throw new BusinessException("Division by zero is not allowed");
            }
        }

        #endregion

        #region Private Helpers

        private IActionResult ExecuteArithmetic(
            ArithmeticRequest request,
            Func<MeasurementRequest, MeasurementRequest, dynamic> operation,
            string operationName)
        {
            ValidateArithmeticRequest(request);

            _logger.LogInformation("{Op} {V1}{U1} and {V2}{U2}",
                operationName,
                request.Quantity1.Value, request.Quantity1.Unit,
                request.Quantity2.Value, request.Quantity2.Unit);

            var record = operation(
                Map(request.Quantity1),
                Map(request.Quantity2));

            if (record.HasError)
                throw new BusinessException(record.ErrorMessage);

            var result = ExtractMeasurement(record.Result);

            return Ok(BuildResponse(result));
        }

        private MeasurementRequest Map(QuantityInputDTO dto) => new()
        {
            Value = dto.Value,
            Unit = dto.Unit,
            Type = dto.Type
        };

        private MeasurementResponse BuildResponse(MeasurementRequest result) => new()
        {
            Value = result.Value,
            Unit = result.Unit,
            Type = result.Type,
            Success = true
        };

        private MeasurementRequest ExtractMeasurement(object result) =>
            result as MeasurementRequest
            ?? throw new BusinessException("Invalid result from service");

        private bool ExtractBooleanResult(object result)
        {
            var measurement = ExtractMeasurement(result);
            return measurement.Value == 1;
        }

        private string BuildCacheKey(ConvertRequest request) =>
            $"convert_{request.Quantity.Value:F4}_" +
            $"{request.Quantity.Unit.ToLower()}_" +
            $"{request.TargetUnit.ToLower()}_" +
            $"{request.Quantity.Type}";

        #endregion

        #region Validation

        private void ValidateCompareRequest(CompareRequest request)
        {
            if (request?.Quantity1 == null || request?.Quantity2 == null)
                throw new BusinessException("Invalid compare request");

            if (request.Quantity1.Type != request.Quantity2.Type)
                throw new BusinessException("Measurement types must match");
        }

        private void ValidateConvertRequest(ConvertRequest request)
        {
            if (request?.Quantity == null || string.IsNullOrWhiteSpace(request.TargetUnit))
                throw new BusinessException("Invalid convert request");
        }

        private void ValidateArithmeticRequest(ArithmeticRequest request)
        {
            if (request?.Quantity1 == null || request?.Quantity2 == null)
                throw new BusinessException("Invalid arithmetic request");

            if (request.Quantity1.Type != request.Quantity2.Type)
                throw new BusinessException("Measurement types must match");
        }

        #endregion
    }
}