using Dapper;
using Npgsql; 
using Microsoft.Extensions.Configuration;
using ModelLayer.DTOs;
using RepositoryLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RepositoryLayer.Implementations
{
    public class QuantityMeasurementDatabaseRepository : IQuantityMeasurementRepository
    {
        private readonly string _connectionString;

        public QuantityMeasurementDatabaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"Database Repository initialized with connection: {_connectionString}");
        }

        public void Save(MeasurementRecord record)
        {
            try
            {
                Console.WriteLine($"\n[DEBUG] Attempting to save record to database...");
                Console.WriteLine($"  Operation: {record.Operation}");
                Console.WriteLine($"  HasError: {record.HasError}");
                
                using var connection = new NpgsqlConnection(_connectionString); 
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@Timestamp", record.Timestamp);
                parameters.Add("@Operation", record.Operation);
                parameters.Add("@HasError", record.HasError);
                parameters.Add("@ErrorMessage", record.ErrorMessage);

                parameters.Add("@Operand1_Value", record.Operand1?.Value);
                parameters.Add("@Operand1_Unit", record.Operand1?.Unit);
                parameters.Add("@Operand1_Type", record.Operand1?.Type);

                if (record.Operand2 != null)
                {
                    parameters.Add("@Operand2_Value", record.Operand2.Value);
                    parameters.Add("@Operand2_Unit", record.Operand2.Unit);
                    parameters.Add("@Operand2_Type", record.Operand2.Type);
                }
                else
                {
                    parameters.Add("@Operand2_Value", null);
                    parameters.Add("@Operand2_Unit", null);
                    parameters.Add("@Operand2_Type", null);
                }

                if (record.Result is MeasurementRequest resultReq)
                {
                    parameters.Add("@Result_Value", resultReq.Value);
                    parameters.Add("@Result_Unit", resultReq.Unit);
                    parameters.Add("@Result_Type", resultReq.Type);
                }
                else
                {
                    parameters.Add("@Result_Value", null);
                    parameters.Add("@Result_Unit", null);
                    parameters.Add("@Result_Type", null);
                }

                const string sql = @"
                    INSERT INTO ""MeasurementRecords"" 
                    (""Timestamp"", ""Operation"", ""HasError"", ""ErrorMessage"",
                     ""Operand1_Value"", ""Operand1_Unit"", ""Operand1_Type"",
                     ""Operand2_Value"", ""Operand2_Unit"", ""Operand2_Type"",
                     ""Result_Value"", ""Result_Unit"", ""Result_Type"")
                    VALUES
                    (@Timestamp, @Operation, @HasError, @ErrorMessage,
                     @Operand1_Value, @Operand1_Unit, @Operand1_Type,
                     @Operand2_Value, @Operand2_Unit, @Operand2_Type,
                     @Result_Value, @Result_Unit, @Result_Type)";

                connection.Execute(sql, parameters);
                Console.WriteLine("  ✓ Record saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ ERROR saving to database: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<MeasurementRecord> GetAll()
        {
            using var connection = new NpgsqlConnection(_connectionString); 
            connection.Open();

            const string sql = @"SELECT * FROM ""MeasurementRecords"" ORDER BY ""Timestamp"" DESC";

            var records = connection.Query<MeasurementRecordDto>(sql);
            return records.Select(r => r.ToMeasurementRecord()).ToList();
        }

        public void Clear()
        {
            using var connection = new NpgsqlConnection(_connectionString); 
            connection.Open();

            const string sql = @"DELETE FROM ""MeasurementRecords""";
            connection.Execute(sql);
        }

        public int GetTotalCount()
        {
            using var connection = new NpgsqlConnection(_connectionString); 
            connection.Open();

            const string sql = @"SELECT COUNT(*) FROM ""MeasurementRecords""";
            return connection.ExecuteScalar<int>(sql);
        }

        public IEnumerable<MeasurementRecord> GetByOperation(string operation)
        {
            using var connection = new NpgsqlConnection(_connectionString); 
            connection.Open();

            const string sql = @"SELECT * FROM ""MeasurementRecords"" WHERE ""Operation"" = @Operation ORDER BY ""Timestamp"" DESC";
            var records = connection.Query<MeasurementRecordDto>(sql, new { Operation = operation });

            return records.Select(r => r.ToMeasurementRecord());
        }
    }

    internal class MeasurementRecordDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Operation { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }

        public double? Operand1_Value { get; set; }
        public string Operand1_Unit { get; set; }
        public string Operand1_Type { get; set; }

        public double? Operand2_Value { get; set; }
        public string Operand2_Unit { get; set; }
        public string Operand2_Type { get; set; }

        public double? Result_Value { get; set; }
        public string Result_Unit { get; set; }
        public string Result_Type { get; set; }

        public MeasurementRecord ToMeasurementRecord()
        {
            var op1 = Operand1_Value.HasValue
                ? new MeasurementRequest { Value = Operand1_Value.Value, Unit = Operand1_Unit, Type = Operand1_Type }
                : null;

            var op2 = Operand2_Value.HasValue
                ? new MeasurementRequest { Value = Operand2_Value.Value, Unit = Operand2_Unit, Type = Operand2_Type }
                : null;

            object result = Result_Value.HasValue
                ? new MeasurementRequest { Value = Result_Value.Value, Unit = Result_Unit, Type = Result_Type }
                : null;

            if (HasError)
            {
                return op2 != null
                    ? new MeasurementRecord(Operation, op1, op2, ErrorMessage)
                    : new MeasurementRecord(Operation, op1, ErrorMessage);
            }

            return op2 != null
                ? new MeasurementRecord(Operation, op1, op2, result)
                : new MeasurementRecord(Operation, op1, result);
        }
    }
}