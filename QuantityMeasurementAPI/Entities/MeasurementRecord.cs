using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantityMeasurementAPI.Entities
{
    [Table("MeasurementRecords")]
    public class MeasurementRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        [MaxLength(50)]
        public string Operation { get; set; } = string.Empty;
        
        // Match exactly what's in your database
        public double? Operand1_Value { get; set; }
        [MaxLength(50)]
        public string? Operand1_Unit { get; set; }
        [MaxLength(50)]
        public string? Operand1_Type { get; set; }
        
        public double? Operand2_Value { get; set; }
        [MaxLength(50)]
        public string? Operand2_Unit { get; set; }
        [MaxLength(50)]
        public string? Operand2_Type { get; set; }
        
        public double? Result_Value { get; set; }
        [MaxLength(50)]
        public string? Result_Unit { get; set; }
        [MaxLength(50)]
        public string? Result_Type { get; set; }
        
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        
        // New columns (optional, will be added via migration)
        public int? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}