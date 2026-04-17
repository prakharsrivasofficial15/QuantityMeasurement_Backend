using ModelLayer.Enums;
using ModelLayer.Extensions;

namespace ModelLayer.Entities
{
    public class Quantity<TEnum> where TEnum : Enum
    {
        private readonly double _value;
        private readonly TEnum _unit;

        public Quantity(double value, TEnum unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));
            
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid value", nameof(value));

            _value = value;
            _unit = unit;
        }

        public double Value => _value;
        public TEnum Unit => _unit;

        private double ConvertToBase()
        {
            return _unit switch
            {
                LengthUnit l => l.ConvertToBaseUnit(_value),
                WeightUnit w => w.ConvertToBaseUnit(_value),
                VolumeUnit v => v.ConvertToBaseUnit(_value),
                TemperatureUnit t => t.ConvertToBaseUnit(_value),
                _ => throw new NotSupportedException($"Unsupported unit type: {typeof(TEnum)}")
            };
        }

        private double ConvertFromBase(double baseValue, TEnum targetUnit)
        {
            return targetUnit switch
            {
                LengthUnit l => l.ConvertFromBaseUnit(baseValue),
                WeightUnit w => w.ConvertFromBaseUnit(baseValue),
                VolumeUnit v => v.ConvertFromBaseUnit(baseValue),
                TemperatureUnit t => t.ConvertFromBaseUnit(baseValue),
                _ => throw new NotSupportedException($"Unsupported unit type: {typeof(TEnum)}")
            };
        }

        public Quantity<TEnum> ConvertTo(TEnum targetUnit)
        {
            if (targetUnit == null)
                throw new ArgumentNullException(nameof(targetUnit));

            double baseValue = ConvertToBase();
            double converted = ConvertFromBase(baseValue, targetUnit);

            return new Quantity<TEnum>(Math.Round(converted, 5), targetUnit);
        }

        public Quantity<TEnum> Add(Quantity<TEnum> other) => Add(other, _unit);
        
        public Quantity<TEnum> Add(Quantity<TEnum> other, TEnum targetUnit)
        {
            ValidateArithmetic(other);
            
            if (_unit is TemperatureUnit)
                throw new NotSupportedException("Temperature measurements cannot be added");
            
            double base1 = ConvertToBase();
            double base2 = other.ConvertToBase();
            double result = base1 + base2;
            double converted = ConvertFromBase(result, targetUnit);
            
            return new Quantity<TEnum>(Math.Round(converted, 5), targetUnit);
        }

        public Quantity<TEnum> Subtract(Quantity<TEnum> other) => Subtract(other, _unit);
        
        public Quantity<TEnum> Subtract(Quantity<TEnum> other, TEnum targetUnit)
        {
            ValidateArithmetic(other);
            
            if (_unit is TemperatureUnit)
                throw new NotSupportedException("Temperature measurements cannot be subtracted");
            
            double base1 = ConvertToBase();
            double base2 = other.ConvertToBase();
            double result = base1 - base2;
            double converted = ConvertFromBase(result, targetUnit);
            
            return new Quantity<TEnum>(Math.Round(converted, 5), targetUnit);
        }

        public double Divide(Quantity<TEnum> other)
        {
            ValidateArithmetic(other);
            
            if (_unit is TemperatureUnit)
                throw new NotSupportedException("Temperature measurements cannot be divided");
            
            double base1 = ConvertToBase();
            double base2 = other.ConvertToBase();
            
            if (base2 == 0)
                throw new DivideByZeroException();
                
            return Math.Round(base1 / base2, 5);
        }

        private void ValidateArithmetic(Quantity<TEnum> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
                
            // Since TEnum is constrained to Enum, both must be of the exact same type
            // The compiler already ensures this through the generic constraint
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Quantity<TEnum> other)
                return false;
                
            return Math.Round(ConvertToBase(), 2) == Math.Round(other.ConvertToBase(), 2);
        }

        public override string ToString()
        {
            string formattedValue = _value % 1 == 0 ? _value.ToString("F0") : _value.ToString("0.##");
            return $"{formattedValue} {_unit}";
        }
        
        public override int GetHashCode() => ConvertToBase().GetHashCode();
    }
}