using ModelLayer.Enums;

namespace ModelLayer.Extensions
{
    public static class UnitConversionExtensions
    {
        // ========== LENGTH CONVERSIONS ==========
        public static double GetConversionFactor(this LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.INCHES => 1.0,
                LengthUnit.FEET => 12.0,
                LengthUnit.YARDS => 36.0,
                LengthUnit.CENTIMETERS => 0.393701,
                _ => throw new ArgumentException("Invalid length unit")
            };
        }

        public static double ConvertToBaseUnit(this LengthUnit unit, double value)
            => value * unit.GetConversionFactor();

        public static double ConvertFromBaseUnit(this LengthUnit unit, double baseValue)
            => baseValue / unit.GetConversionFactor();

        // ========== WEIGHT CONVERSIONS ==========
        public static double GetConversionFactor(this WeightUnit unit)
        {
            return unit switch
            {
                WeightUnit.MILLIGRAM => 0.001,
                WeightUnit.GRAM => 1.0,
                WeightUnit.KILOGRAM => 1000.0,
                WeightUnit.POUND => 453.592,
                WeightUnit.TONNE => 1000000.0,
                _ => throw new ArgumentException("Invalid weight unit")
            };
        }

        public static double ConvertToBaseUnit(this WeightUnit unit, double value)
            => Math.Round(value * unit.GetConversionFactor(), 2);

        public static double ConvertFromBaseUnit(this WeightUnit unit, double baseValue)
            => Math.Round(baseValue / unit.GetConversionFactor(), 2);

        // ========== VOLUME CONVERSIONS ==========
        public static double GetConversionFactor(this VolumeUnit unit)
        {
            return unit switch
            {
                VolumeUnit.MILLILITRE => 0.001,
                VolumeUnit.LITRE => 1.0,
                VolumeUnit.GALLON => 3.78541,
                _ => throw new ArgumentException("Invalid volume unit")
            };
        }

        public static double ConvertToBaseUnit(this VolumeUnit unit, double value)
            => value * unit.GetConversionFactor();

        public static double ConvertFromBaseUnit(this VolumeUnit unit, double baseValue)
            => baseValue / unit.GetConversionFactor();

        // ========== TEMPERATURE CONVERSIONS ==========
        public static double ConvertToBaseUnit(this TemperatureUnit unit, double value)
        {
            return unit switch
            {
                TemperatureUnit.CELSIUS => value,
                TemperatureUnit.FAHRENHEIT => (value - 32) * 5.0 / 9.0,
                _ => throw new ArgumentException("Invalid temperature unit")
            };
        }

        public static double ConvertFromBaseUnit(this TemperatureUnit unit, double baseValue)
        {
            return unit switch
            {
                TemperatureUnit.CELSIUS => baseValue,
                TemperatureUnit.FAHRENHEIT => (baseValue * 9 / 5) + 32,
                _ => throw new ArgumentException("Invalid temperature unit")
            };
        }

        // Temperature does NOT support arithmetic
        public static bool SupportsArithmetic(this TemperatureUnit unit) => false;

        // ========== COMMON HELPER ==========
        public static string GetUnitName(this Enum unit) => unit.ToString();
    }
}